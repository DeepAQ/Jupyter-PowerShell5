using System;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
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
                var rs = RunspaceFactory.CreateRunspace(InitialSessionState.CreateDefault2());
                // var rs = RunspaceFactory.CreateOutOfProcessRunspace(new TypeTable(Enumerable.Empty<string>()));
                rs.Open();
                var t = new Thread(() =>
                {
                    try
                    {
                        var ps = PowerShell.Create();
                        ps.Runspace = rs;
                        var completion = CommandCompletion.CompleteInput("Get-Content -T", 14, null, ps);
                        // foreach (var psObject in ps.AddScript("(TabExpansion2 -inputScript 'Get-Content -T' -cursorColumn 14).CompletionMatches").AddCommand("Out-String").Invoke())
                        // {
                        //     Console.WriteLine(psObject);
                        // }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
                t.Start();
                t.Join();

                rs.Close();
            }
        }
    }
}