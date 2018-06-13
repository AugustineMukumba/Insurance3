using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    class VehicleUsageModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string VehUsage { get; set; }
        public Single ComprehensiveRate { get; set; }
        public decimal MinCompAmount { get; set; }
        public Single ThirdPartyRate { get; set; }
        public decimal MinThirdAmount { get; set; }
        public decimal FTPAmount { get; set; }
        public decimal AnnualTPAmount { get; set; }
    }
}
