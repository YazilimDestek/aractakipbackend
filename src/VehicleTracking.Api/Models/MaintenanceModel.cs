using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleTracking.Api.Models
{
    public class MaintenanceModel
    {
        public int? VehicleTypeId { get; set; }
        public long MaintenanceMileage { get; set; } // bakim kilometre
        public string MaintenanceType { get; set; } // Bakım Türü (Periyodik Bakım, Lastik Bakımı)
        public DateTime RememberDate { get; set; } // bakim hatirlatici
        public string MaintenancePath { get; set; } // Mail sms ya da her ikisi burayi json olarak tutabiliriz.
    }
}
