using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleTracking.Api.Models
{
    public class VehicleTireHistoryFilterModel
    {
        public int? TireId { get; set; }
        public int? VehicleId { get; set; }
        public DateTime? RemovedDateStart { get; set; } // sokuldugu tarih (null ise sökülmemiş demektir)
        public DateTime RemovedDateEnd { get; set; }
        public DateTime InstalledDateStart { get; set; } // takildigi tarih
        public DateTime InstalledDateEnd { get; set; }
        public bool isOnVehicle { get; set; }
    }
}
