using System;
using System.Collections.Generic;
using System.Text;

namespace VehicleTracking.Entity.Models
{
    public class VehicleType:BaseEntity
    {
        public string Name { get; set; } // (Oto, Kamyon, Ağır Vasıta, İş Makinesi ...)
        public string SubType { get; set; } // (Ekskavatör, Yükleyici, Loder ...)
    }
}
