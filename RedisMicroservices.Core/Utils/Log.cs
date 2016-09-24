using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RedisMicroservices.Core.Utils
{
    public static class Log
    {
        static object _lock = new object();

        public static void Write(string msg)
        {
            Task.Run(() =>
            {
                try
                {
                    string file;
                    var fileName = DateTime.Now.ToString("yyyy-MM-dd-hh") + ".log";
                    try
                    {
                        file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                    }
                    catch
                    {
                        file = HttpContext.Current.Server.MapPath("~/" + fileName);
                    }
                    lock (_lock)
                    {
                        using (var sw = new StreamWriter(file, true))
                        {
                            sw.WriteLine(msg);
                            sw.Flush();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });
        }
    }
}