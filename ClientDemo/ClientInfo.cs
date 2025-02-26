using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientDemo
{
    public class ClientInfo
    {
        public string UserName { get; set; }
        public string SystemVer { get; set; }
        public DateTime ConnectTime { get; set; }
        public uint ProcessID { get; set; }
        public string RunPath { get; set; }
        public string FileName { get; set; }

        public ClientInfo()
        {
            UserName = Environment.UserName;
            SystemVer = Environment.OSVersion.ToString();
            ConnectTime = DateTime.Now;
            ProcessID = (uint)System.Diagnostics.Process.GetCurrentProcess().Id;
            RunPath = AppDomain.CurrentDomain.BaseDirectory;
            FileName = Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
        }

        public override string ToString()
        {
            return $"<ClientInfo>\n" +
                   $"  <UserName>{UserName}</UserName>\n" +
                   $"  <SystemVer>{SystemVer}</SystemVer>\n" +
                   $"  <ConnectTime>{ConnectTime}</ConnectTime>\n" +
                   $"  <ProcessID>{ProcessID}</ProcessID>\n" +
                   $"  <RunPath>{RunPath}</RunPath>\n" +
                   $"  <FileName>{FileName}</FileName>\n" +
                   $"</ClientInfo>";
        }

    }



    }
