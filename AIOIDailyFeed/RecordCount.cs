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
    internal class RecordCount
    {
        public static int NewRecordCount()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AioiVehicleDetails"].ToString();
            int recordCount;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }

                using (SqlCommand cmd = connection.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@refId", "refId_int");
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT COUNT (@refId) FROM tabAIOIVehicleDetails;";
                    recordCount = (int)cmd.ExecuteScalar();
                }

            }

            return recordCount;
        }
    }
}
