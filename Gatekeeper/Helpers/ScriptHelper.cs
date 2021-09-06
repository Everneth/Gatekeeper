using System.Diagnostics;

namespace Gatekeeper.Helpers
{
    public static class ScriptHelper
    {
        // Starts a new process by calling via bash to execute a script specified in arguments
        public static void Run(string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = "/bin/bash", Arguments = arguments };
            Process proc = new Process() { StartInfo = startInfo };
            proc.Start();
        }
    }
}
