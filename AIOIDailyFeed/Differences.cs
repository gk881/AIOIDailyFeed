using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Globalization;

namespace AIOIDailyFeed
{
    public class Differences
    {
        public static List<Vehicle> Regs(DataTable dt, DataTable oldRegistration, bool endCover)
        {
            var newTable = new DataTable();

            List<Vehicle> oldList = oldRegistration.Rows.OfType<DataRow>().Select(dr => ConvertToVehicleOld(dr)).ToList();
            List<Vehicle> newList = dt.Rows.OfType<DataRow>().Select(dr => ConvertToVehicleNew(dr)).ToList();


            if (!endCover)
            {
                var differentList = oldList.Except(newList);

                return differentList.ToList();
            }
            else
            {
                var differentList = newList.Except(oldList);

                return differentList.ToList();
            }

        }

        private static Vehicle ConvertToVehicleOld(DataRow row)
        {
            Vehicle vehicle = new Vehicle();

            vehicle.registration = row.Field<string>("registrationNumber_char");
            vehicle.coverEndDate = row.Field<DateTime>("coverEndDate_date");


            return vehicle;
        }

        public static Vehicle ConvertToVehicleNew(DataRow row)
        {
            Vehicle vehicle = new Vehicle();
            var DT = row.Field<string>("Cover_End_Date");
            DateTime date; 

            vehicle.registration = row.Field<string>("Registration_Number");
            //vehicle.coverEndDate = DateTime.ParseExact(DT, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            if (DateTime.TryParse(DT, out date))
            {
                vehicle.coverEndDate = date;
            }
            else
            {
                vehicle.coverEndDate = DateTime.ParseExact(DT, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }


            return vehicle;
        }



    }
}
