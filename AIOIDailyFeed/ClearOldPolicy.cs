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
    public class ClearOldPolicy
    {


        public static void Remove(List<Vehicle> oldRegList)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AioiVehicleDetails"].ToString();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open(); 
                }

                using (SqlCommand cmd = connection.CreateCommand())
                {
         
                    if (oldRegList.Count > 1)
                    {

                        foreach (Vehicle veh in oldRegList)
                        {
                            if (veh.coverEndDate > DateTime.Now)
                            {

                                cmd.Parameters.AddWithValue("@registration", veh.registration);
                                cmd.Parameters.AddWithValue("@offCoverDate", veh.coverEndDate);

                                cmd.CommandText = "UPDATE tabAIOIVehicleDetails SET coverEndDate_date = CAST(CAST(GETDATE() AS DATE) AS DATETIME) WHERE registrationNumber_char = @registration " +
                                                    "AND coverEndDate_date > GETDATE(); ";
                                cmd.ExecuteNonQuery();
                                cmd.Parameters.Clear();
                                Console.WriteLine("Ended cover for " + veh.registration); 
                            }
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

        //public static List<string> OldRegs(DataTable dt, DataTable oldRegistration, bool endCover)
        //{

        //    var newTable = new DataTable();

        //   List<string> oldList = oldRegistration.Rows.OfType<DataRow>().Select(dr => dr.Field<string>("registrationNumber_char")).ToList();
        //    List<string> newList = dt.Rows.OfType<DataRow>().Select(dr => dr.Field<string>("Registration_Number")).ToList();


        //    if (!endCover)
        //    {
        //        var differentList = oldList.Except(newList);

        //        return differentList.ToList();
        //    }
        //    else
        //    {
        //        var differentList = newList.Except(oldList);

        //        return differentList.ToList();
        //    }

        //}


    }
}


