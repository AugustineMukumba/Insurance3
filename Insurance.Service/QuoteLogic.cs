using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Insurance.Domain;
using InsuranceClaim.Models;
using System.Configuration;

namespace Insurance.Service
{
    public class QuoteLogic
    {
        public decimal Premium { get; set; }
        public decimal StamDuty { get; set; }
        public decimal ZtscLevy { get; set; }
        public bool Status { get; set; } = true;
        public string Message { get; set; }






        public QuoteLogic CalculatePremium(int vehicleUsageId, decimal sumInsured, eCoverType coverType, eExcessType excessType, decimal excess, int PaymentTermid, decimal? AddThirdPartyAmount, int NumberofPersons, Boolean Addthirdparty, Boolean PassengerAccidentCover, Boolean ExcessBuyBack, Boolean RoadsideAssistance, Boolean MedicalExpenses, decimal? RadioLicenseCost, int? AgentCommissionId)
        {
            var vehicleUsage = InsuranceContext.VehicleUsages.Single(vehicleUsageId);
            var Setting = InsuranceContext.Settings.All();
            var AgentCommission = InsuranceContext.AgentCommissions.Single(AgentCommissionId).CommissionAmount;
            var additionalchargeatp = 0.0m;
            var additionalchargepac = 0.0m;
            var additionalchargeebb = 0.0m;
            var additionalchargersa = 0.0m;
            var additionalchargeme = 0.0m;
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
            else if (coverType == eCoverType.FullThirdParty)
            {
                InsuranceRate = (float)vehicleUsage.FTPAmount;
                InsuranceMinAmount = vehicleUsage.FTPAmount;
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
            else if (coverType == eCoverType.FullThirdParty)
            {
                premium = (decimal)InsuranceRate;
            }
            else
            {
                premium = (sumInsured * Convert.ToDecimal(InsuranceRate)) / 100;
            }


            if (premium < InsuranceMinAmount && coverType == eCoverType.Comprehensive)
            {
                Status = false;
                //premium = premium + InsuranceMinAmount.Value;
                premium = InsuranceMinAmount.Value;
                this.Message = "Insurance minimum amount $" + InsuranceMinAmount + " Charge is applicable.";
            }

            switch (PaymentTermid)
            {
                case 3:
                    premium = premium / 4;
                    break;
                case 4:
                    premium = premium / 3;
                    break;
            }


            if (Addthirdparty)
            {
                var AddThirdPartyAmountADD = AddThirdPartyAmount;

                if (AddThirdPartyAmountADD > 10000)
                {
                    var settingAddThirdparty = Convert.ToDecimal(Setting.Where(x => x.keyname == "Addthirdparty").Select(x => x.value).FirstOrDefault());
                    var Amount = AddThirdPartyAmountADD - 10000;
                    premium += Convert.ToDecimal((Amount * settingAddThirdparty) / 100);

                }
            }
            if (PassengerAccidentCover)
            {
                int additionalAmountPerPerson = Convert.ToInt32(Setting.Where(x => x.keyname == "PassengerAccidentCover").Select(x => x.value).FirstOrDefault());

                int totalAdditionalPACcharge = NumberofPersons * additionalAmountPerPerson;

                additionalchargepac = totalAdditionalPACcharge;

            }
            if (ExcessBuyBack)
            {

                int additionalAmountExcessBuyBack = Convert.ToInt32(Setting.Where(x => x.keyname == "ExcessBuyBack").Select(x => x.value).FirstOrDefault());


                additionalchargeebb = (premium * additionalAmountExcessBuyBack) / 100;


            }
            if (RoadsideAssistance)
            {
                decimal additionalAmountRoadsideAssistance = Convert.ToDecimal(Setting.Where(x => x.keyname == "RoadsideAssistance").Select(x => x.value).FirstOrDefault());


                additionalchargersa = (premium * additionalAmountRoadsideAssistance) / 100;


            }
            if (MedicalExpenses)
            {

                decimal additionalAmountMedicalExpenses = Convert.ToDecimal(Setting.Where(x => x.keyname == "MedicalExpenses").Select(x => x.value).FirstOrDefault());


                additionalchargeme = (premium * additionalAmountMedicalExpenses) / 100;

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
            this.StamDuty = Math.Round(stampDuty, 2);
            this.ZtscLevy = Math.Round(ztscLevy, 2);



            premium = premium + stampDuty + ztscLevy;



            premium = premium + additionalchargeebb + additionalchargeme + additionalchargepac + additionalchargersa + Convert.ToDecimal(RadioLicenseCost);// + Convert.ToDecimal(AgentCommission);

            this.Premium = Math.Round(premium, 2);





            return this;
        }




        //public QuoteLogic CalculatePremium(int vehicleUsageId, decimal sumInsured, eCoverType coverType, eExcessType excessType, decimal excess, int PaymentTermid, decimal? AddThirdPartyAmount, int NumberofPersons, Boolean Addthirdparty, Boolean PassengerAccidentCover, Boolean ExcessBuyBack, Boolean RoadsideAssistance, Boolean MedicalExpenses)
        //{
        //    var vehicleUsage = InsuranceContext.VehicleUsages.Single(vehicleUsageId);
        //    var Setting = InsuranceContext.Settings.All();
        //    var additionalchargeatp = 0.0m;
        //    var additionalchargepac = 0.0m;
        //    var additionalchargeebb = 0.0m;
        //    var additionalchargersa = 0.0m;
        //    var additionalchargeme = 0.0m;
        //    float? InsuranceRate = 0;
        //    decimal? InsuranceMinAmount = 0;
        //    if (coverType == eCoverType.Comprehensive)
        //    {
        //        InsuranceRate = vehicleUsage.ComprehensiveRate;
        //        InsuranceMinAmount = vehicleUsage.MinCompAmount;
        //    }
        //    else if (coverType == eCoverType.ThirdParty)
        //    {
        //        InsuranceRate = (float)vehicleUsage.AnnualTPAmount;
        //        InsuranceMinAmount = vehicleUsage.MinThirdAmount;
        //    }
        //    if (excessType == eExcessType.Percentage && excess > 0)
        //    {
        //        InsuranceRate = InsuranceRate + float.Parse(excess.ToString());
        //    }

        //    var premium = 0.00m;

        //    if (coverType == eCoverType.ThirdParty)
        //    {
        //        premium = (decimal)InsuranceRate;
        //    }
        //    else
        //    {
        //        premium = (sumInsured * Convert.ToDecimal(InsuranceRate)) / 100;
        //    }



        //    if (Addthirdparty)
        //    {
        //        var AddThirdPartyAmountADD = AddThirdPartyAmount;

        //        if (AddThirdPartyAmountADD > 10000)
        //        {
        //            var settingAddThirdparty = Convert.ToDecimal(Setting.Where(x => x.key == "Addthirdparty").Select(x => x.value).FirstOrDefault());
        //            var Amount = AddThirdPartyAmountADD - 10000;
        //            premium += Convert.ToDecimal((Amount * settingAddThirdparty) / 100);

        //        }
        //    }
        //    if (PassengerAccidentCover)
        //    {
        //        int additionalAmountPerPerson = Convert.ToInt32(Setting.Where(x => x.key == "PassengerAccidentCover").Select(x => x.value).FirstOrDefault());

        //        int totalAdditionalPACcharge = NumberofPersons * additionalAmountPerPerson;

        //        additionalchargepac = totalAdditionalPACcharge;

        //    }
        //    if (ExcessBuyBack)
        //    {

        //        int additionalAmountExcessBuyBack = Convert.ToInt32(Setting.Where(x => x.key == "ExcessBuyBack").Select(x => x.value).FirstOrDefault());


        //        additionalchargeebb = (premium * additionalAmountExcessBuyBack) / 100;


        //    }
        //    if (RoadsideAssistance)
        //    {
        //        decimal additionalAmountRoadsideAssistance = Convert.ToDecimal(Setting.Where(x => x.key == "RoadsideAssistance").Select(x => x.value).FirstOrDefault());


        //        additionalchargersa = (premium * additionalAmountRoadsideAssistance) / 100;


        //    }
        //    if (MedicalExpenses)
        //    {

        //        decimal additionalAmountMedicalExpenses = Convert.ToDecimal(Setting.Where(x => x.key == "MedicalExpenses").Select(x => x.value).FirstOrDefault());


        //        additionalchargeme = (premium * additionalAmountMedicalExpenses) / 100;

        //    }




        //    //if (sumInsured > 10000)
        //    //{
        //    //    var extraamount = sumInsured - 10000m;
        //    //    var additionalcharge = ((0.5 * (double)extraamount) / 100);
        //    //    premium = premium + (decimal)additionalcharge;
        //    //}
        //    if (premium < InsuranceMinAmount && coverType == eCoverType.Comprehensive)
        //    {
        //        Status = false;
        //        //premium = premium + InsuranceMinAmount.Value;
        //        premium = InsuranceMinAmount.Value;
        //        this.Message = "Insurance minimum amount $" + InsuranceMinAmount + " Charge is applicable.";
        //    }

        //    //if (premium < InsuranceMinAmount)
        //    //{
        //    //    Status = false;
        //    //    //premium = premium + InsuranceMinAmount.Value;
        //    //    premium = InsuranceMinAmount.Value;
        //    //    this.Message = "Insurance minimum amount $" + InsuranceMinAmount + " Charge is applicable.";
        //    //}



        //    if (excessType == eExcessType.FixedAmount && excess > 0)
        //    {
        //        premium = premium + excess;
        //    }

        //    this.Premium = premium;
        //    var stampDuty = (premium * 5) / 100;
        //    if (stampDuty > 2000000)
        //    {

        //        Status = false;
        //        this.Message = "Stamp Duty must not exceed $2,000,000";
        //    }

        //    var ztscLevy = (premium * 12) / 100;
        //    this.StamDuty = Math.Round(stampDuty, 2);
        //    this.ZtscLevy = Math.Round(ztscLevy, 2);



        //    premium = premium + stampDuty + ztscLevy ;

        //    switch (PaymentTermid)
        //    {
        //        case 3:
        //            premium = premium / 4;
        //            break;
        //        case 4:
        //            premium = premium / 3;
        //            break;
        //    }

        //    premium = premium + additionalchargeebb + additionalchargeme + additionalchargepac + additionalchargersa;

        //    this.Premium = Math.Round(premium, 2);





        //    return this;
        //}
    }


}
