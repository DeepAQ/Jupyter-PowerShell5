using System;
using System.IO;
using System.Management.Automation;
using Jupyter_PowerShell5.Models;
using Newtonsoft.Json;

namespace Jupyter_PowerShell5
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                Console.WriteLine($"Connection file: {args[0]}");
                var connection = JsonConvert.DeserializeObject<Connection>(File.ReadAllText(args[0]));
                var kernel = new Kernel(connection);
                kernel.Start();
                kernel.Wait();
            }
            else
            {
                var ps = PowerShell.Create();
                foreach (var psObject in ps.AddScript("$PSVersionTable").AddCommand("Out-String").Invoke())
                {
                    Console.WriteLine(psObject);
                }

                foreach (var informationRecord in ps.Streams.Information)
                {
                    Console.WriteLine(informationRecord.MessageData.ToString());
                }
            }
        }
    }
}