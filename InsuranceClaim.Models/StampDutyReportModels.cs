using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
   public class StampDutyReportModels
    {
        public string Customer_Name { get; set; }
        public string Policy_Number { get; set; }
        public string Transaction_date { get; set; }
        public decimal Premium_due { get; set; }
        public decimal Stamp_duty { get; set; }
    }
    public class ListStampDutyReportModels
    {
        public List<StampDutyReportModels> ListStampDutyReportdata { get; set; }
    }
}
