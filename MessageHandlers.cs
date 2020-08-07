using System.Reflection;
using Jupyter_PowerShell5.Models;

namespace Jupyter_PowerShell5
{
    public static class MessageHandlers
    {
        public static void HandleKernelInfo(Kernel kernel, Message message)
        {
            kernel.ShellSocket.SendMessage(Message.Create(
                message, "kernel_info_reply", new KernelInfoReply
                {
                    Status = StatusBase.Ok,
                    ProtocolVersion = "5.3",
                    Implementation = "powershell5",
                    ImplementationVersion = "1.0.0",
                    LanguageInfo = new LanguageInfo
                    {
                        Name = "powershell",
                        Version = "5.1.0",
                        MimeType = "text/plain"
                    },
                    Banner = $"Windows PowerShell 5.1 (CLR {Assembly.GetExecutingAssembly().ImageRuntimeVersion})"
                }), kernel);
        }
    }
}