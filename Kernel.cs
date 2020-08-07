using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jupyter_PowerShell5.Models;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json.Linq;

namespace Jupyter_PowerShell5
{
    public class Kernel
    {
        public delegate void HandleMessage(Kernel kernel, Message message);

        public ResponseSocket HBSocket { get; set; }
        public RouterSocket ShellSocket { get; set; }
        public PublisherSocket IOPubSocket { get; set; }

        private readonly Connection connection;
        private readonly HMAC signatureProvider;
        private Task heartbeatTask;
        private Task messageTask;
        private bool running = false;

        public Kernel(Connection connection)
        {
            this.connection = connection;
            if (!string.IsNullOrEmpty(this.connection.Key))
            {
                this.signatureProvider =
                    HMAC.Create(this.connection.SignatureScheme.Replace("-", string.Empty).ToUpperInvariant());
                this.signatureProvider.Key = Encoding.ASCII.GetBytes(this.connection.Key);
            }
        }

        public void Start()
        {
            lock (this)
            {
                this.Stop();
                Console.WriteLine("Starting kernel");

                Volatile.Write(ref this.running, true);
                this.heartbeatTask = Task.Run(this.ProcessHeartbeat);
                this.messageTask = Task.Run(this.ProcessMessage);
            }
        }

        public void Wait()
        {
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
                    Task.WaitAll(this.heartbeatTask, this.messageTask);
                }
            }
        }

        public string SignMessage(params string[] parts)
        {
            this.signatureProvider.Initialize();
            foreach (var item in parts)
            {
                var bytes = Encoding.UTF8.GetBytes(item);
                this.signatureProvider.TransformBlock(bytes, 0, bytes.Length, null, 0);
            }

            this.signatureProvider.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            return BitConverter.ToString(this.signatureProvider.Hash).Replace("-", string.Empty).ToLowerInvariant();
        }

        private void ProcessHeartbeat()
        {
            var hbAddress = this.GetAddress(this.connection.HBPort);
            Console.WriteLine($"Starting heartbeat at {hbAddress}");
            this.HBSocket = new ResponseSocket(hbAddress);

            while (Volatile.Read(ref this.running))
            {
                Console.WriteLine("~ Heartbeat");
                this.HBSocket.SendFrame(this.HBSocket.ReceiveFrameBytes());
            }

            this.HBSocket.Dispose();
        }

        private void ProcessMessage()
        {
            var shellAddress = this.GetAddress(this.connection.ShellPort);
            var ioAddress = this.GetAddress(this.connection.IOPubPort);
            Console.WriteLine($"Starting shell server at {shellAddress}");
            Console.WriteLine($"Starting IO publisher at {ioAddress}");
            this.ShellSocket = new RouterSocket(shellAddress);
            this.IOPubSocket = new PublisherSocket(ioAddress);

            var globalSessionId = Guid.NewGuid().ToString();
            this.IOPubSocket.SendMessage(Message.Create(
                globalSessionId, "status", new Status {ExecutionState = Status.Starting}), this);

            while (Volatile.Read(ref this.running))
            {
                var message = this.ShellSocket.ReceiveMessage(this);
                this.IOPubSocket.SendMessage(Message.Create(
                    message, "status", new Status {ExecutionState = Status.Busy}), this);

                if (!string.IsNullOrEmpty(message.Signature))
                {
                    switch (message.Header.MessageType)
                    {
                        case "kernel_info_request":
                            MessageHandlers.HandleKernelInfo(this, message);
                            break;
                    }
                }

                this.IOPubSocket.SendMessage(Message.Create(
                    message, "status", new Status {ExecutionState = Status.Idle}), this);
            }

            this.ShellSocket.Dispose();
            this.IOPubSocket.Dispose();
        }

        private string GetAddress(int port)
        {
            return $"{this.connection.Transport}://{this.connection.IP}:{port}";
        }
    }
}