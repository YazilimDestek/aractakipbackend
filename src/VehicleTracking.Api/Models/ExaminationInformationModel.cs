using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleTracking.Api.Models
{
    public class ExaminationInformationModel
    {
        public int VehicleId { get; set; }
        public DateTime ExaminationDate { get; set; } // Muayene Tarihi
        public string ExaminationResult { get; set; } // Muayene sonucu 
        public string ExaminationResultDocument { get; set; }
    }
}
