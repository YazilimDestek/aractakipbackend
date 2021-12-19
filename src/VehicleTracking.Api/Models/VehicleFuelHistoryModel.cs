using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleTracking.Api.Models
{
    public class VehicleFuelHistoryModel
    {
        public int? VehicleId { get; set; }
        public string FuelType { get; set; } 
        public int Liter { get; set; } 
        public DateTime TakenDate { get; set; } 
        public long Mileage { get; set; } 
    }
}
