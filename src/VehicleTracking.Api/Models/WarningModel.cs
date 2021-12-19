using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleTracking.Api.Models
{
    public class WarningModel
    {
        public int? WarningTypeId { get; set; } // Uyarı Tipi
        public string Name { get; set; } // Uyarı Adı
        public string Description { get; set; } // Uyarı Tanımı
        public int DaysLeft { get; set; } // Uyarı kaç gün kala gelecek
        public string WarningMethod { get; set; } // Uyarı Şekli
    }
}
