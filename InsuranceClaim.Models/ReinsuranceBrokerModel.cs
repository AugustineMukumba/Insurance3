using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
   public class ReinsuranceBrokerModel
    {

        public int Id { get; set; }
        public string ReinsuranceBrokerCode { get; set; }
        public string ReinsuranceBrokerName { get; set; }
        public decimal? Commission { get; set; }

    }
}
