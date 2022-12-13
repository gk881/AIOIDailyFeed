//AIOIDailyFeed V2

using System.Diagnostics;

//Check there isn't already an instance running 
if (Process.GetProcessesByName("AIOIDailyFeed").Length > 1)
{
    return;
}
else
{
    AIOIDailyFeed.Log.WriteLine("Process Started");

    AIOIDailyFeed.FTPAccess.ConnectToFTP();

    int records = AIOIDailyFeed.RecordCount.NewRecordCount();

    AIOIDailyFeed.Main.ReadIn(records);

    AIOIDailyFeed.Log.WriteLine("Process Complete  ");

}