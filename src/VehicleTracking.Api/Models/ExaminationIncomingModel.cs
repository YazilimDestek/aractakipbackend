using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using VehicleTracking.Entity.Models;

namespace VehicleTracking.Api.Models
{
    public class ExaminationIncomingModel
    {
        public Examination Maintenance { get; set; }
        public Vehicle Vehicle { get; set; }
        public long Remaining { get; set; }
        public bool Alert { get; set; }
    }
}
