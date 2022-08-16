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

    int records = AIOIDailyFeed.RecordCount.NewRecordCount();

    // this condition was initially developed incase of additional data being added onto bottom of existing spreadsheet - will only add the new rows and ignore existing
   //If records already exist (and they should) UpdateRun() 
    //if (records > 0)
    //{
    //    AIOIDailyFeed.UpdateRun.ReadIn(records);
    //    AIOIDailyFeed.Log.WriteLine("Process Complete");
    //}

    //else
    //{
        AIOIDailyFeed.Main.ReadIn();
    //}

    //AIOIDailyFeed.ClearOldPolicy.Remove();

    AIOIDailyFeed.Log.WriteLine("Process Complete  ");


}
