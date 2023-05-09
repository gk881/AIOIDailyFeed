using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace AIOIDailyFeed
{
    internal class RemoveDupe
    {
        //Only really needed on applications first run - this initially populates the empty table 

        public static void DuplicateCheck()
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
                    cmd.CommandText = "SELECT registrationNumber_char FROM tabAioiVehicleDetails GROUP BY registrationNumber_char HAVING count(*) > 1"; 
                }
            }

           
        }
    }
}