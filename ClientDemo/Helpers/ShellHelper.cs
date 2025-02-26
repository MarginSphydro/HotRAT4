using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace ClientDemo.Helpers
{
    public static class ShellHelper
    {
        public static async Task<string> ExecuteCommandAsync(string command, bool usePowerShell = false)
        {
            var cmd = $"{command.Replace(" {amp;} ", "&").Replace(" {plus;} ", "+").Replace(" {eq;} ", "=").Replace(" {quest;} ", "?")}";
            Console.WriteLine(cmd);
            var processStartInfo = new ProcessStartInfo
            {
                FileName = usePowerShell ? "powershell.exe" : "cmd.exe",
                Arguments = $"/c \"{cmd}\"",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            var process = new Process { StartInfo = processStartInfo };

            try
            {
                process.Start();

                // 获取进程的 PID
                var pid = process.Id;

                await process.StandardInput.FlushAsync();
                process.StandardInput.Close();

                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                process.WaitForExit();

                // 有错误信息则返回；否则返回包含PID的输出
                return string.IsNullOrEmpty(error)
                    ? $"{pid}\\n{output.Replace("\n", "\\n")}"
                    : $"{pid}\\nError: {error.Replace("\n", "\\n")}";
            }
            finally
            {
                process.Dispose();
            }
        }

    }

}
