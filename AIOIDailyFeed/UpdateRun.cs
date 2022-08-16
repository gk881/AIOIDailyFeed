using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ExcelDataReader;
using System.Data;
using System.Data.SqlClient;
using System.Configuration; 

namespace AIOIDailyFeed
{
    internal class UpdateRun
    {
        public static void ReadIn(int records)
        {
            try
            {
                bool runSuccess = false;

                foreach (string file in Directory.GetFiles(ConfigurationSettings.AppSettings["Path"].ToString()))
                {

                    DataTableCollection tableCollection;
                    DataTable dt = new DataTable();
                    DataSet result = new DataSet();

                    using (var stream = File.Open(file, FileMode.Open, FileAccess.Read))
                    {
                        if (file.Contains(".xlsx") || file.Contains(".csv"))
                        {
                            runSuccess = true;

                            if (file.Contains(".xlsx"))
                            {
                                using (var reader = ExcelReaderFactory.CreateReader(stream))
                                {
                                    result = reader.AsDataSet(new ExcelDataSetConfiguration()
                                    {
                                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { FilterRow = rowReader => rowReader.Depth > records, UseHeaderRow = true }
                                    });
                                }
                            }
                            else if (file.Contains(".csv"))
                            {
                                using (var reader = ExcelReaderFactory.CreateCsvReader(stream))
                                {
                                    result = reader.AsDataSet(new ExcelDataSetConfiguration()
                                    {
                                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { FilterRow = rowReader => rowReader.Depth > records, UseHeaderRow = true }
                                    });
                                }
                            }
                            tableCollection = result.Tables;
                            dt = tableCollection[0];

                            Main.Write(dt);
                            stream.Close();
                            Main.CleanUp(file);
                        }
                    }
                }
                        
                if (!runSuccess)
                {
                    Log.WriteLine("Error. No source file found"); 
                }

            }
            catch (Exception ex)
            {
                Log.WriteLine("Error. Could not read excel file: " + ex.Message); 
            }
        }
    }

}
