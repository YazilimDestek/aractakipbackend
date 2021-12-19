using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VehicleTracking.Entity.Models
{
    public class MaintenanceResult:BaseEntity
    {
        [ForeignKey("Vehicle")]
        public int? VehicleId { get; set; } 
        public Vehicle Vehicle { get; set; }
        public string MaintenanceType { get; set; }
        public  long MaintenanceMileage { get; set; }
        public DateTime MaintenanceDate { get; set; }
    }
}
