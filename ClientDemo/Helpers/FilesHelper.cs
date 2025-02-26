using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientDemo.Helpers
{
    internal class FilesHelper
    {
        public static string GetFiles(string path)
        {
            string[] files = Directory.GetFiles(path);
            string[] dirs = Directory.GetDirectories(path);
            string result = "";
            foreach (string dir in dirs)
            {
                DirectoryInfo dinfo = new(dir);
                result += $"{dir}&{dinfo.CreationTime}&{dinfo.LastWriteTime}&Unknow" + "\n";
            }
            foreach (string file in files)
            {
                FileInfo finfo = new(file);
                result += $"{file}&{finfo.CreationTime}&{finfo.LastWriteTime}&{GetFileSize(Path.Combine(path,file))}" + "\n";
            }
            return result;
        }

        public static long GetFileSize(string filePath)
        {
            if (File.Exists(filePath))
            {
                FileInfo fileInfo = new FileInfo(filePath);
                return fileInfo.Length / 1024; //kb
            }
            else
            {
                throw new FileNotFoundException("指定的文件不存在。", filePath);
            }
        }
    }
}
