using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleTracking.Api.Models
{
    public class TrafficFineModel
    {
        public int? VehicleId { get; set; }
        public int? StaffId { get; set; } // Aracı kullanan Personal Idsi;
        public double Price { get; set; }
        public string Description { get; set; }
        public DateTime IssueDate { get; set; } // Veriliş Tarihi
        public DateTime LastPaymentDate { get; set; } // Son ödeme Tarihi
        public bool IsPayed { get; set; } // Ödendi mi
        public bool IsReported { get; set; } // Muhasebeye bildirildi mi 
    }
}
