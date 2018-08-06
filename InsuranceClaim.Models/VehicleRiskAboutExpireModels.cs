using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
  public  class VehicleRiskAboutExpireModels
    {
        public string Customer_Name { get; set; }
        public string phone_number { get; set; }
        public string Policy_Number { get; set; }
        public string Transaction_date { get; set; }
        public string Vehicle_startdate { get; set; }
        public string Vehicle_makeandmodel { get; set; }
        public string Vehicle_enddate { get; set; }
        public decimal Premium_due { get; set; }
        public decimal Sum_Insured { get; set; }
    }
    public class ListVehicleRiskAboutExpireModels
    {
        public List<VehicleRiskAboutExpireModels> ListVehicleRiskAboutExpiredata { get; set; }
    }
}
