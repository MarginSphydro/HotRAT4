using HotRAT.Server.Configs;
using HotRAT.Server.Models;
using Newtonsoft.Json;
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

        public static void Main(string[] args)
        {
            Test();
            LoggerModel.Initialize();
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            Task.Run(async() =>
            {
                using var server = new SocketServer(8080);
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
