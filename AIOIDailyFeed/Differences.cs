using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace AIOIDailyFeed
{
    public class Differences
    {
        public static List<string> Regs(DataTable dt, DataTable oldRegistration, bool endCover)
        {
            var newTable = new DataTable();

            List<string> oldList = oldRegistration.Rows.OfType<DataRow>().Select(dr => dr.Field<string>("registrationNumber_char")).ToList();
            List<string> newList = dt.Rows.OfType<DataRow>().Select(dr => dr.Field<string>("Registration_Number")).ToList();


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
    }
}
