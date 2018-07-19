using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
public class VehicleListModel
    {
        public string make { get; set; }
        public string model { get; set; }
        public string covertype { get; set; }
        public string suminsured { get; set; }
        public string premium { get; set; }
    }
}
