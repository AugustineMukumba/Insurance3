using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Insurance.Domain;
using InsuranceClaim.Models;

namespace Insurance.Service
{
    public class QuoteLogic
    {
        public decimal Premium { get; set; }
        public decimal StamDuty { get; set; }
        public decimal ZtscLevy { get; set; }
        public bool Status { get; set; } = true;
        public string Message { get; set; }

        public QuoteLogic CalculatePremium(int vehicleUsageId, decimal sumInsured, eCoverType coverType, eExcessType excessType, decimal excess)
        {
            var vehicleUsage = InsuranceContext.VehicleUsages.Single(vehicleUsageId);
            float? InsuranceRate = 0;
            decimal? InsuranceMinAmount = 0;
            if (coverType == eCoverType.Comprehensive)
            {
                InsuranceRate = vehicleUsage.ComprehensiveRate;
                InsuranceMinAmount = vehicleUsage.MinCompAmount;
            }
            else if (coverType == eCoverType.ThirdParty)
            {
                InsuranceRate = vehicleUsage.ThirdPartyRate;
                InsuranceMinAmount = vehicleUsage.MinThirdAmount;
            }
            if (excessType == eExcessType.Percentage && excess > 0) 
            {
                InsuranceRate = InsuranceRate + float.Parse(excess.ToString());
            }
            var premium = (sumInsured * Convert.ToDecimal(InsuranceRate)) / 100;
            if (excessType == eExcessType.FixedAmount && excess > 0)
            {
                premium = premium + excess;
            }
            if (premium < InsuranceMinAmount)
            {
                Status = false;
                premium = premium + InsuranceMinAmount.Value;
                this.Message = "Insurance minimum amount $" + InsuranceMinAmount + " Charge is applicable.";
            }
            this.Premium = premium;
            var stampDuty = (premium * 5) / 100;
            if (stampDuty > 2000000)
            {

                Status = false;
                this.Message = "Stamp Duty must not exceed $2,000,000";
            }

            var ztscLevy = (premium * 12) / 100;
            this.StamDuty = stampDuty;
            this.ZtscLevy = ztscLevy;
            return this;
        }
    }


}
