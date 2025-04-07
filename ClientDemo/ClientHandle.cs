using ClientDemo.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientDemo
{
    public class ClientHandle
    {
        public async static void Handle(string code,string data)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"代办码 - {code}\n指令 - {data.Replace("\n", "\\n")}");
                Console.ResetColor();

                if (code == "CONNECT")
                {
                    Program.SessionId = data;
                    Console.WriteLine($"信息 - 已经通过中转服务器与主机建立连接 | 会话ID - {data}");
                    return;
                }
                if (code == "DISCONNECT")
                {
                    Program.SessionId = "";
                    Console.WriteLine($"信息 - 主机已断开连接 | 会话ID - {data}");
                    return;
                }
                else if (Program.SessionId != "")
                {
                    var dataL = data.Split("\n");
                    Console.WriteLine(dataL[0]);
                    switch (dataL[0].Replace("\n", ""))
                    {
                        case "香辣鸡腿堡":
                            await Program.client.SendDataAsync(2, BuildResult(Program.SessionId, code, "一个香辣鸡腿堡8块钱人民币."), AuthModel.Token());
                            break;
                        case "FRAME":
                            //var resolution = dataL[1];
                            string scResult = ScreenHelper.CaptureScreenAsBase64();
                            await Program.client.SendDataAsync(2, BuildResult(Program.SessionId, code, scResult), AuthModel.Token());
                            
                            break;
                        case "SHELL":
                            if (dataL.Length >= 2)
                            {
                                var command = dataL[1];
                                string psResult = await ShellHelper.ExecuteCommandAsync(command, usePowerShell: true);
                                await Program.client.SendDataAsync(2, BuildResult(Program.SessionId, code, psResult), AuthModel.Token());

                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"Output:\n{psResult}");
                                Console.ResetColor();
                            }
                            break;
                        case "FILES":
                            if (dataL.Length >= 2)
                            {
                                string fileResult = "";
                                if (dataL[1] == "HOME")
                                {
                                    DriveInfo[] drives = DriveInfo.GetDrives();
                                    foreach (DriveInfo drive in drives)
                                    {
                                        if (drive.IsReady)
                                        {
                                            long totalSize = drive.TotalSize / (1024 * 1024);
                                            long usedSpace = (drive.TotalSize - drive.AvailableFreeSpace) / (1024 * 1024);

                                            fileResult += $"{drive.Name}&{drive.VolumeLabel}&{totalSize}&{usedSpace}\\n";
                                        }
                                    }

                                }
                                else
                                {
                                    fileResult = FilesHelper.GetFiles(dataL[1]).Replace("\n", "\\n");
                                }

                                await Program.client.SendDataAsync(2, BuildResult(Program.SessionId, code, fileResult), AuthModel.Token());

                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"Output:\n{fileResult}");
                                Console.ResetColor();
                            }
                            break;
                        default:

                            break;
                    }
                }
                else
                {
                    Console.WriteLine("会话无效");
                }
            }
            catch(Exception ex)
            {
                await Program.client.SendDataAsync(2, BuildResult(Program.SessionId, code, ex.Message), AuthModel.Token());
            }

           
        }

        public static string BuildResult(string sessionID,string waitCode,string result) => $"{sessionID}\n{waitCode}\n{result}";
    }
}
