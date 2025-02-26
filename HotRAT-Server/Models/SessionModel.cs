using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotRAT_Server.Models
{
    public class MessageModel
    {
        public int State { get; set; }
        public string Code { get; set; }
        public string Content { get; set; }
        public string Result { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;
    }

    public class SessionModel
    {
        public string Id { get; set; }
        public string Token { get; set; }
        public DateTime CreatTime {  get; set; } = DateTime.Now;
        public string ClientIp { get; set; }
        public string ClientPort { get; set; }
        public MessageModel[] Historys { get; set; }

    }
}
