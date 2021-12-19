using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleTracking.Api.Models
{
    public class VehicleTypeModel
    {
        public string Name { get; set; } // (Oto, Kamyon, Ağır Vasıta, İş Makinesi ...)
        public string SubType { get; set; } // (Ekskavatör, Yükleyici, Loder ...)
    }
}
