﻿//AIOIDailyFeed V2

using System.Diagnostics;

//Check there isn't already an instance running 
if (Process.GetProcessesByName("AIOIDailyFeed").Length > 1)
{
    return;
}
else
{
    AIOIDailyFeed.Log.WriteLine("Process Started");

#if DEBUG
#else
    AIOIDailyFeed.FTPAccess.ConnectToFTP();
#endif

    int records = AIOIDailyFeed.RecordCount.NewRecordCount();

    AIOIDailyFeed.Main.ReadIn(records);

    //AIOIDailyFeed.RemoveDupe.DuplicateCheck(); 

    AIOIDailyFeed.Log.WriteLine("Process Complete  "); 

}