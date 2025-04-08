using System.Net;
using System.Reflection;
using System.Text;

namespace HotRAT.WSServer
{
    public static class Cnm{
        public static void sendMessage(this string str)
        {
            Console.WriteLine(str);
        }
    }
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            "死妈广东狗".sendMessage();
            Console.WriteLine("Hello, World!");
        }

    }
}
