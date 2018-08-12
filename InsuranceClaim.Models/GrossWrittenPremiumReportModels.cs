using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
  public  class GrossWrittenPremiumReportModels
    {
        public string Customer_Name { get; set; }
        public string Policy_Number { get; set; }
        public string Policy_endate { get; set; }
        public string Policy_startdate { get; set; }
        public string Transaction_date { get; set; }
        public string Vehicle_makeandmodel { get; set; }
        public string Payment_Mode { get; set; }
        public string Payment_Term { get; set; }
        public decimal Annual_Premium { get; set;}
        public decimal Stamp_duty { get; set; }
        public decimal ZTSC_Levy { get; set; }
        public decimal? Net_Premium { get; set; }
        public decimal Premium_due { get; set; }
        public decimal Comission_percentage { get; set; }
        public decimal Comission_Amount { get; set; }
        public decimal Sum_Insured { get; set; }
        public decimal? RadioLicenseCost { get; set; }
    }
    public class ListGrossWrittenPremiumReportModels
    {
        public List<GrossWrittenPremiumReportModels> ListGrossWrittenPremiumReportdata { get; set; }
    }
}
