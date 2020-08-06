using System.Linq;
using System.Text;
using Jupyter_PowerShell5.Models;
using NetMQ;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Jupyter_PowerShell5
{
    public static class SocketExtensions
    {
        private static readonly byte[] Delimiter = Encoding.ASCII.GetBytes("<IDS|MSG>");

        public static Message ReceiveMessage(this IReceivingSocket socket)
        {
            var message = new Message();
            while (true)
            {
                var bytes = socket.ReceiveFrameBytes();
                if (bytes.SequenceEqual(Delimiter))
                {
                    break;
                }

                message.Identities.Add(bytes);
            }

            message.Signature = socket.ReceiveFrameString();
            message.Header = JsonConvert.DeserializeObject<Header>(socket.ReceiveFrameString());
            message.ParentHeader = JsonConvert.DeserializeObject<Header>(socket.ReceiveFrameString());
            message.MetaData = JObject.Parse(socket.ReceiveFrameString());
            message.Content = JObject.Parse(socket.ReceiveFrameString());
            return message;
        }
    }
}