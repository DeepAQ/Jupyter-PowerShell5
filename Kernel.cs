using System;
using System.Threading;
using System.Threading.Tasks;
using Jupyter_PowerShell5.Models;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace Jupyter_PowerShell5
{
    public class Kernel
    {
        private readonly Connection connection;
        private Task heartbeatTask;
        private Task messageTask;

        private bool running = false;

        public Kernel(Connection connection)
        {
            this.connection = connection;
        }

        public void Start()
        {
            lock (this)
            {
                this.Stop();
                Console.WriteLine("Starting kernel with connection:");
                Console.WriteLine(JsonConvert.SerializeObject(this.connection));

                Volatile.Write(ref this.running, true);
                this.heartbeatTask = Task.Run(this.ProcessHeartbeat);
                this.messageTask = Task.Run(this.ProcessMessage);
            }

            Task.WaitAll(this.heartbeatTask, this.messageTask);
        }

        public void Stop()
        {
            lock (this)
            {
                if (Volatile.Read(ref this.running))
                {
                    Console.WriteLine("Stopping kernel");
                    Volatile.Write(ref this.running, false);
                }
            }
        }

        private void ProcessHeartbeat()
        {
            var hbAddress = this.GetAddress(this.connection.HBPort);
            Console.WriteLine($"Starting heartbeat at {hbAddress}");
            using var hbSocket = new ResponseSocket(hbAddress);

            while (Volatile.Read(ref this.running))
            {
                hbSocket.SendFrame(hbSocket.ReceiveFrameBytes());
            }
        }

        private void ProcessMessage()
        {
            var shellAddress = this.GetAddress(this.connection.ShellPort);
            var ioAddress = this.GetAddress(this.connection.IOPubPort);
            Console.WriteLine($"Starting shell server at {shellAddress}");
            Console.WriteLine($"Starting IO publisher at {ioAddress}");
            using var shellSocket = new RouterSocket(shellAddress);
            using var ioSocket = new PublisherSocket(ioAddress);

            while (Volatile.Read(ref this.running))
            {
                var message = shellSocket.ReceiveMessage();
                Console.WriteLine($"> {JsonConvert.SerializeObject(message)}");
            }
        }

        private string GetAddress(int port)
        {
            return $"{this.connection.Transport}://{this.connection.IP}:{port}";
        }
    }
}