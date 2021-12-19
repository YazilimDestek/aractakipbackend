using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleTracking.Api.Models
{
    public class VehicleTireHistoryModel
    {
        public int? TireId { get; set; }
        public int? VehicleId { get; set; }
        public int? HoleOrder { get; set; } // kaçıncı lastik yerine takıldı? sol önden başlayarak saat yönünde sıra numarası.
        public DateTime? RemovedDate { get; set; } // sokuldugu tarih (null ise sökülmemiş demektir)
        public DateTime InstalledDate { get; set; } // takildigi tarih
        public string RemovedReason { get; set; } // sokulme nedeni
    }
}
