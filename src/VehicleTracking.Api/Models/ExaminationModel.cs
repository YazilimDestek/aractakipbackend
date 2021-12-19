using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleTracking.Api.Models
{
    public class ExaminationModel
    {
        public int? VehicleTypeId { get; set; }
        public long Mileage { get; set; }
    }
}
