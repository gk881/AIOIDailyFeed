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
        public static void Remove()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AioiVehicleDetails"].ToString();
            int polsRemoved = 0; 

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open(); 
                }

            }


        }
    }
}
