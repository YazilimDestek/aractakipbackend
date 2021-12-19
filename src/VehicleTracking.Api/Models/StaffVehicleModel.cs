using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleTracking.Api.Models
{
    public class StaffVehicleModel
    {
        public int? StaffId { get; set; }
        public int? VehicleId { get; set; }

        public bool IsPermanent { get; set; }
        public bool IsDaily { get; set; }
        public bool IsWeek { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
