using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class LapsedPoliciesReportModels
    {
        public string customerName { get; set; }
        public string contactDetails { get; set; }
        public string vehicleMake { get; set; }
        public string vehicleModel { get; set; }
        public decimal? sumInsured { get; set; }
        public decimal? Premium { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
    }
}
