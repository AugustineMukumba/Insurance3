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

        public QuoteLogic CalculatePremium(int vehicleUsageId, decimal sumInsured, eCoverType coverType, eExcessType excessType, decimal excess,int PaymentTermid)
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
                InsuranceRate = (float)vehicleUsage.AnnualTPAmount;
                InsuranceMinAmount = vehicleUsage.MinThirdAmount;
            }
            if (excessType == eExcessType.Percentage && excess > 0) 
            {
                InsuranceRate = InsuranceRate + float.Parse(excess.ToString());
            }

            var premium = 0.00m;

            if (coverType == eCoverType.ThirdParty)
            {
                premium = (decimal)InsuranceRate;
            }
            else
            {
                premium = (sumInsured * Convert.ToDecimal(InsuranceRate)) / 100;
            }

            if (sumInsured > 10000)
            {
                var extraamount = sumInsured - 10000m;
                var additionalcharge = ((0.5 * (double)extraamount) / 100);
                premium = premium + (decimal)additionalcharge;
            }
            if (premium < InsuranceMinAmount && coverType == eCoverType.Comprehensive)
            {
                Status = false;
                //premium = premium + InsuranceMinAmount.Value;
                premium = InsuranceMinAmount.Value;
                this.Message = "Insurance minimum amount $" + InsuranceMinAmount + " Charge is applicable.";
            }

            //if (premium < InsuranceMinAmount)
            //{
            //    Status = false;
            //    //premium = premium + InsuranceMinAmount.Value;
            //    premium = InsuranceMinAmount.Value;
            //    this.Message = "Insurance minimum amount $" + InsuranceMinAmount + " Charge is applicable.";
            //}

            switch (PaymentTermid)
            {                
                case 3:
                    premium = premium / 4;
                    break;
                case 4:
                    premium = premium / 3;
                    break;
            }

            if (excessType == eExcessType.FixedAmount && excess > 0)
            {
                premium = premium + excess;
            }
          
            this.Premium = premium;
            var stampDuty = (premium * 5) / 100;
            if (stampDuty > 2000000)
            {

                Status = false;
                this.Message = "Stamp Duty must not exceed $2,000,000";
            }

            var ztscLevy = (premium * 12) / 100;
            this.StamDuty =Math.Round(stampDuty,2);
            this.ZtscLevy =Math.Round(ztscLevy,2);

            premium = premium + stampDuty + ztscLevy;
            this.Premium =Math.Round(premium,2);
            return this;
        }
    }


}
