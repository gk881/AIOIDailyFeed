using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIOIDailyFeed
{
    internal class Log
    {

        public static void WriteLine(String value)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(DateTime.Now.ToString());
            sb.Append(" - ");
            sb.Append(value);
            Console.WriteLine(sb.ToString());
            WriteLogFile(sb.ToString());
        }

        private static void WriteLogFile(String value)
        {
        String logPath = @"C:\Program Files\brokerdirect\AIOIArchive\";
            String timeStamp = DateTime.Now.ToString("yyyy-MM-dd");

            using (StreamWriter sw = new StreamWriter(logPath + timeStamp + ".txt", true))
            {
                sw.WriteLine(value);
            }
        }
    } 
}
