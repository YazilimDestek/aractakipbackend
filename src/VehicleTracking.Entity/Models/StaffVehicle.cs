using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VehicleTracking.Entity.Models
{
    public class StaffVehicle : BaseEntity
    {
        [ForeignKey("Staff")]
        public int? StaffId { get; set; }
        public Staff Staff { get; set; }

        [ForeignKey("Vehicle")]
        public int? VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }

        public bool IsPermanent { get; set; }
        public bool IsDaily { get; set; }
        public bool IsWeek { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
