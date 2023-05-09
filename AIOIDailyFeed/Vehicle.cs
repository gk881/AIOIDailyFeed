using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIOIDailyFeed
{
    public class Vehicle
    {
        public string registration { get; set; }
        public DateTime? coverEndDate { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj.GetType() == typeof(Vehicle))
            {
                var vehicle = obj as Vehicle;
                if (vehicle.registration == registration && vehicle.coverEndDate == coverEndDate)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
