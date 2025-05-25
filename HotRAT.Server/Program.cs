using HotRAT.Server.Configs;
using HotRAT.Server.Models;
using HotRAT.WSServer.Models;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;
using System.Text;

namespace HotRAT.Server
{
    public class Program
    {
        static void Test()
        {
            Console.WriteLine(ConfigModels.serverConfig.Key);
            Console.WriteLine(Runtimes.Token());
            Console.WriteLine(Libs.Auth.TokenModel.Build(ConfigModels.serverConfig.Key));
        }

        public static async Task Main(string[] args)
        {
            Test();
            LoggerModel.Initialize();
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .SetIsOriginAllowed(_ => true);
                });
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseCors("AllowAll");

            app.UseAuthorization();
            app.MapControllers();

            Runtimes.CWrite("WebSokcet 主控服务器启动，端口[8081]", ConsoleColor.Green);
            _ = Runtimes.WSserver.StartAsync();

            _ = Task.Run(async () =>
            {
                using var server = new SocketServer(8080);
                Runtimes.CWrite("Socket 被控服务器启动，端口[8080]", ConsoleColor.Green);
                await server.StartAsync();
            });

            app.Run();
        }

        public static string ReadFile(string file)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourcePath = $"HotRAT.Server.{file}";

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
