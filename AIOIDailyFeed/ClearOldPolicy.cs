using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace AIOIDailyFeed
{
    internal class ClearOldPolicy
    {
        public static void Remove(DataTable dt)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AioiVehicleDetails"].ToString();
            var oldRegistration = new DataTable(); 

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open(); 
                }

                using (SqlCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT * FROM tabAIOIVehicleDetails;";
                    cmd.ExecuteNonQuery();

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(oldRegistration);
                    da.Dispose();

                    if (oldRegistration.Rows.Count > 1)
                    {

                        var differenceDataSet = ClearOldPolicy.OldRegs(dt, oldRegistration);

                        foreach (string reg in differenceDataSet)
                        {
                            
                                cmd.Parameters.AddWithValue("@registration", reg);
                                cmd.Parameters.AddWithValue("@offCoverDate", DateTime.Now.ToString("yyyy-MM-dd 00:00:00:000"));

                                cmd.CommandText = "UPDATE tabAIOIVehicleDetails SET coverEndDate_date = @offCoverDate WHERE registrationNumber_char = @registration " +
                                                    "AND coverEndDate_date > GETDATE(); ";
                                cmd.ExecuteNonQuery();
                                Log.WriteLine(reg + " cover ended.");
                                cmd.Parameters.Clear();
                        }
                        
                    }
                    else
                    {
                        Log.WriteLine("No data found in tabAIOIVehicleData - Should only happen on initial run.");
                    }

                    connection.Close(); 
                }

            }

        }

        private static List<string> OldRegs(DataTable dt, DataTable oldRegistration)
        {

            var newTable = new DataTable();

            List<string> oldList = oldRegistration.Rows.OfType<DataRow>().Select(dr => dr.Field<string>("registrationNumber_char")).ToList();
            List<string> newList = dt.Rows.OfType<DataRow>().Select(dr => dr.Field<string>("Registration_Number")).ToList();

            var differentList = oldList.Except(newList);


            return differentList.ToList(); 
        }


    }
}


