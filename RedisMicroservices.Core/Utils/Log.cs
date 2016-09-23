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
        public static void Write(string msg)
        {
            string file;
            var fileName = DateTime.Now.ToString("yyyy-MM-dd-hh") + ".log";
            try
            {
                file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            }
            catch (Exception)
            {
                file = HttpContext.Current.Server.MapPath("~/" + fileName);
            }

            using (var sw = new StreamWriter(file, true))
            {
                sw.WriteLine(msg);
                sw.Flush();
            }
        }
    }
}