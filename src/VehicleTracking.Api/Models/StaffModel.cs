using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleTracking.Api.Models
{
    public class StaffModel
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int? UserId { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string AbysCode { get; set; }
        public bool IsTrackingEnable { get; set; }
    }
}
