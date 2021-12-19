using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;
using System.Text;

namespace VehicleTracking.Entity.Models
{
    public class Maintenance : BaseEntity
    {
        [ForeignKey("VehicleType")]
        public int? VehicleTypeId { get; set; }
        public VehicleType VehicleType { get; set; } // Arac tipi secimi
        public long MaintenanceMileage { get; set; } // bakim kilometre
        public string MaintenanceType { get; set; } // Bakım Türü (Periyodik Bakım, Lastik Bakımı)
        public DateTime RememberDate { get; set; } // bakim hatirlatici
        public  string MaintenancePath { get; set; } // Mail sms ya da her ikisi burayi json olarak tutabiliriz.

    }
}
