using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using WinSCP;

namespace AIOIDailyFeed
{
    internal class FTPAccess
    {

        public static void ConnectToFTP()
        {
            SessionOptions sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Sftp,
                HostName = "bd-ftp.brokerdirect.co.uk",
                UserName = "aioi",
                Password = "344XYq83h1m4uQ5FtcNx",
                SshHostKeyFingerprint = "ssh-rsa 1024 E5d33/KEHr5PRgfIDr/nBmHD/4pE8McL5/O2XwSs/vg=",
            };

            using (Session session = new Session())
            {
                string fileDate = DateTime.Now.ToString("yyyyMMdd"); 

                session.Open(sessionOptions);
                session.GetFiles(@"brokerdirect_"+fileDate+".csv", ConfigurationSettings.AppSettings["Path"].ToString()+"brokerdirect_"+fileDate+".csv").Check();
                Log.WriteLine("Downloaded " + "to " + ConfigurationSettings.AppSettings["Path"].ToString());
                session.RemoveFile(@"brokerdirect_" + fileDate + ".csv"); 
            }

        }

    }
}