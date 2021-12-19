using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleTracking.Api.Models
{
    public class MaintenanceResultModel
    {
        public int? VehicleId { get; set; }
        public string MaintenanceType { get; set; }
        public long MaintenanceMileage { get; set; }
        public DateTime MaintenanceDate { get; set; }
    }
}
