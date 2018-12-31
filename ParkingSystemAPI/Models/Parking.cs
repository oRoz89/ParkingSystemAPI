using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ParkingSystemAPI.Models
{
    public class Parking
    {
        public int ID { get; set; }
        public String CustomerSlug { get; set; }
        public int Operater { get; set; }
        public DateTime EnterTime { get; set; }
        public DateTime ExitTime { get; set; }

    }
}