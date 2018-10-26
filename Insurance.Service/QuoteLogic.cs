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
        public decimal ExcessBuyBackAmount { get; set; }
        public decimal RoadsideAssistanceAmount { get; set; }
        public decimal MedicalExpensesAmount { get; set; }
        public decimal PassengerAccidentCoverAmount { get; set; }
        public decimal PassengerAccidentCoverAmountPerPerson { get; set; }
        public decimal ExcessBuyBackPercentage { get; set; }
        public decimal RoadsideAssistancePercentage { get; set; }
        public decimal MedicalExpensesPercentage { get; set; }
        public decimal ExcessAmount { get; set; }
        public decimal AnnualRiskPremium { get; set; }
        public decimal TermlyRiskPremium { get; set; }
        public decimal QuaterlyRiskPremium { get; set; }
        public decimal Discount { get; set; }

        public QuoteLogic CalculatePremium(int vehicleUsageId, decimal sumInsured, eCoverType coverType, eExcessType excessType, decimal excess, int PaymentTermid, decimal? AddThirdPartyAmount, int NumberofPersons, Boolean Addthirdparty, Boolean PassengerAccidentCover, Boolean ExcessBuyBack, Boolean RoadsideAssistance, Boolean MedicalExpenses, decimal? RadioLicenseCost, Boolean IncludeRadioLicenseCost, Boolean isVehicleRegisteredonICEcash, string BasicPremiumICEcash, string StampDutyICEcash, string ZTSCLevyICEcash, int ProductId = 0)
        {
            var vehicleUsage = InsuranceContext.VehicleUsages.Single(vehicleUsageId);
            var Setting = InsuranceContext.Settings.All();
            var DiscountOnRenewalSettings = Setting.Where(x => x.keyname == "Discount On Renewal").FirstOrDefault();

            var additionalchargeatp = 0.0m;
            var additionalchargepac = 0.0m;
            var additionalchargeebb = 0.0m;
            var additionalchargersa = 0.0m;
            var additionalchargeme = 0.0m;
            float? InsuranceRate = 0;
            decimal? InsuranceMinAmount = 0;
            this.AnnualRiskPremium = 0.00m;
            this.QuaterlyRiskPremium = 0.00m;
            this.TermlyRiskPremium = 0.00m;
            this.Discount = 0.00m;


            if (coverType == eCoverType.Comprehensive)
            {
                InsuranceRate = vehicleUsage.ComprehensiveRate;
                InsuranceMinAmount = vehicleUsage.MinCompAmount;
            }
            else if (coverType == eCoverType.ThirdParty)
            {
                InsuranceRate = vehicleUsage.AnnualTPAmount == null ? 0 : (float)vehicleUsage.AnnualTPAmount;
                InsuranceMinAmount = vehicleUsage.MinThirdAmount;
            }
            else if (coverType == eCoverType.FullThirdParty)
            {
                InsuranceRate = (float)vehicleUsage.FTPAmount;
                InsuranceMinAmount = vehicleUsage.FTPAmount;
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


            var settingAddThirdparty = Convert.ToDecimal(Setting.Where(x => x.keyname == "Addthirdparty").Select(x => x.value).FirstOrDefault());

            if (Addthirdparty)
            {
                var AddThirdPartyAmountADD = AddThirdPartyAmount;

                if (AddThirdPartyAmountADD > 10000)
                {
                    var Amount = AddThirdPartyAmountADD - 10000;
                    premium += Convert.ToDecimal((Amount * settingAddThirdparty) / 100);

                }
            }


            int day = 0;
            double calulateTerm = 0;
            switch (PaymentTermid)
            {
                case 3:
                    premium = premium / 4;
                    break;
                case 4:
                    premium = premium / 3;
                    break;
                case 5:
                     day = 5*30;
                    premium = Math.Round(Convert.ToDecimal((double)day / 365) * premium, 2);
                    break;
                case 6:
                    day = 6 * 30;
                    premium = Math.Round( Convert.ToDecimal((double)day / 365) * premium, 2);
                    break;
                case 7:
                    day = 7 * 30;
                    premium = Math.Round(Convert.ToDecimal((double)day / 365) * premium, 2);
                    break;
                case 8:
                    day = 8 * 30;
                    premium = Math.Round(Convert.ToDecimal((double)day / 365) * premium, 2);
                    break;
                case 9:
                    day = 9 * 30;
                    premium = Math.Round(Convert.ToDecimal((double)day / 365) * premium, 2);
                    break;
                case 10:
                    day = 10 * 30;
                    premium = Math.Round(Convert.ToDecimal((double)day / 365) * premium, 2);
                    break;
                case 11:
                    day = 11 * 30;
                    premium = Math.Round(Convert.ToDecimal((double)day / 365) * premium, 2);
                    break;
            }


           

            decimal PassengerAccidentCoverAmountPerPerson = Convert.ToInt32(Setting.Where(x => x.keyname == "PassengerAccidentCover").Select(x => x.value).FirstOrDefault());
            decimal ExcessBuyBackPercentage = Convert.ToInt32(Setting.Where(x => x.keyname == "ExcessBuyBack").Select(x => x.value).FirstOrDefault());
            decimal RoadsideAssistancePercentage = Convert.ToDecimal(Setting.Where(x => x.keyname == "RoadsideAssistance").Select(x => x.value).FirstOrDefault());
            decimal MedicalExpensesPercentage = Convert.ToDecimal(Setting.Where(x => x.keyname == "MedicalExpenses").Select(x => x.value).FirstOrDefault());
            var StampDutySetting = Setting.Where(x => x.keyname == "Stamp Duty").FirstOrDefault();
            var ZTSCLevySetting = Setting.Where(x => x.keyname == "ZTSC Levy").FirstOrDefault();

           


            if (PassengerAccidentCover)
            {
                int totalAdditionalPACcharge = NumberofPersons * Convert.ToInt32(PassengerAccidentCoverAmountPerPerson);
                additionalchargepac = totalAdditionalPACcharge;
            }

            if (ExcessBuyBack)
            {
                additionalchargeebb = (sumInsured * ExcessBuyBackPercentage) / 100;
            }

            if (RoadsideAssistance)
            {
                if ((coverType == eCoverType.ThirdParty || coverType == eCoverType.FullThirdParty) && ProductId == 1) // for private car
                {
                    var roadsideAssistanceDetails = Setting.Where(x => x.keyname == "third party private cars roadside assistance").FirstOrDefault();
                    if (roadsideAssistanceDetails != null)
                    {
                        additionalchargersa = Math.Round(Convert.ToDecimal(roadsideAssistanceDetails.value), 2);
                    }
                }
                else if ((coverType == eCoverType.ThirdParty || coverType == eCoverType.FullThirdParty) && ProductId == 3) // Commercial vehicles
                {
                    var roadsideAssistanceDetails = Setting.Where(x => x.keyname == "third party commercial vehicle roadside assistance").FirstOrDefault();
                    if (roadsideAssistanceDetails != null)
                    {
                        additionalchargersa = Math.Round(Convert.ToDecimal(roadsideAssistanceDetails.value), 2);
                    }
                }
                else
                {
                    // it's for compresnsive
                    additionalchargersa = (sumInsured * RoadsideAssistancePercentage) / 100;
                }
            }

            if (MedicalExpenses)
            {
                additionalchargeme = (sumInsured * MedicalExpensesPercentage) / 100;
            }

            if (excessType == eExcessType.FixedAmount && excess > 0)
            {
                this.ExcessAmount = excess;
            }

            this.Premium = premium;

            this.PassengerAccidentCoverAmount = Math.Round(additionalchargepac, 2);
            this.PassengerAccidentCoverAmountPerPerson = Math.Round(PassengerAccidentCoverAmountPerPerson, 2);
            this.RoadsideAssistanceAmount = Math.Round(additionalchargersa, 2);

            this.RoadsideAssistancePercentage = Math.Round(RoadsideAssistancePercentage, 2);
            this.MedicalExpensesAmount = Math.Round(additionalchargeme, 2);
            this.MedicalExpensesPercentage = Math.Round(MedicalExpensesPercentage, 2);
            this.ExcessBuyBackAmount = Math.Round(additionalchargeebb, 2);
            this.ExcessBuyBackPercentage = Math.Round(ExcessBuyBackPercentage, 2);

            //premium = premium + stampDuty + ztscLevy;
            //premium = premium + additionalchargeebb + additionalchargeme + additionalchargepac + additionalchargersa + (IncludeRadioLicenseCost ? Convert.ToDecimal(RadioLicenseCost) : 0.00m );// + Convert.ToDecimal(AgentCommission);
            //if (excessType == eExcessType.Percentage && excess > 0)
            //{
            //    InsuranceRate = InsuranceRate + float.Parse(excess.ToString());
            //}

            if (excessType == eExcessType.Percentage && excess > 0)
            {
                this.ExcessAmount = (sumInsured * excess) / 100;
            }


            //decimal totalPremium = 0;
            //decimal calCulatePremiumWithOptional = 0;

            //if (coverType == eCoverType.Comprehensive)
            //{
            //    calCulatePremiumWithOptional = this.Premium + this.PassengerAccidentCoverAmount + this.RoadsideAssistanceAmount + this.MedicalExpensesAmount + this.ExcessBuyBackAmount + this.ExcessAmount;
            //}
            //else
            //{
            //    calCulatePremiumWithOptional = (isVehicleRegisteredonICEcash ? Convert.ToDecimal(BasicPremiumICEcash) : this.Premium) + this.PassengerAccidentCoverAmount + this.RoadsideAssistanceAmount + this.MedicalExpensesAmount + this.ExcessBuyBackAmount + this.ExcessAmount;
            //}




            //switch (PaymentTermid)
            //{
            //    case 3:
            //        premium = calCulatePremiumWithOptional / 4;
            //        break;
            //    case 4:
            //        premium = calCulatePremiumWithOptional / 3;
            //        break;
            //}


            var discountField= this.PassengerAccidentCoverAmount + this.RoadsideAssistanceAmount + this.MedicalExpensesAmount + this.ExcessBuyBackAmount + this.ExcessAmount;

            switch (PaymentTermid)
            {
                case 3:
                    discountField = discountField / 4;
                    break;
                case 4:
                    discountField = discountField / 3;
                    break;
                case 5:
                    day = 5 * 30;
                    discountField = Math.Round(Convert.ToDecimal((double)day / 365) * discountField, 2);
                    break;
                case 6:
                    day = 6 * 30;
                    discountField = Math.Round(Convert.ToDecimal((double)day / 365) * discountField, 2);
                    break;
                case 7:
                    day = 7 * 30;
                    discountField = Math.Round(Convert.ToDecimal((double)day / 365) * discountField, 2);
                    break;
                case 8:
                    day = 8 * 30;
                    discountField = Math.Round(Convert.ToDecimal((double)day / 365) * discountField, 2);
                    break;
                case 9:
                    day = 9 * 30;
                    discountField = Math.Round(Convert.ToDecimal((double)day / 365) * discountField, 2);
                    break;
                case 10:
                    day = 10 * 30;
                    discountField = Math.Round(Convert.ToDecimal((double)day / 365) * discountField, 2);
                    break;
                case 11:
                    day = 11 * 30;
                    discountField = Math.Round(Convert.ToDecimal((double)day / 365) * discountField, 2);
                    break;
            }


            switch (PaymentTermid)
            {
                case 1:
                    this.AnnualRiskPremium = premium+ discountField;
                    if (isVehicleRegisteredonICEcash && !(coverType == eCoverType.Comprehensive))
                    {
                        this.AnnualRiskPremium = Convert.ToDecimal(BasicPremiumICEcash);
                    }
                    if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                    {
                        this.Discount = ((this.AnnualRiskPremium * Convert.ToDecimal(DiscountOnRenewalSettings.value)) / 100);
                    }
                    if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.amount))
                    {
                        this.Discount = Convert.ToDecimal(DiscountOnRenewalSettings.value);
                    }
                    break;
                case 3:
                    this.QuaterlyRiskPremium = premium+ discountField;
                    if (isVehicleRegisteredonICEcash && !(coverType == eCoverType.Comprehensive))
                    {
                        this.QuaterlyRiskPremium = Convert.ToDecimal(BasicPremiumICEcash);
                    }
                    if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                    {
                        this.Discount = ((this.QuaterlyRiskPremium * Convert.ToDecimal(DiscountOnRenewalSettings.value)) / 100);
                    }
                    if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.amount))
                    {
                        this.Discount = Convert.ToDecimal(DiscountOnRenewalSettings.value);
                    }
                    break;
                case 4:
                    this.TermlyRiskPremium = premium+ discountField;
                    if (isVehicleRegisteredonICEcash && !(coverType == eCoverType.Comprehensive))
                    {
                        this.TermlyRiskPremium = Convert.ToDecimal(BasicPremiumICEcash);
                    }
                    if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                    {
                        this.Discount = ((this.TermlyRiskPremium * Convert.ToDecimal(DiscountOnRenewalSettings.value)) / 100);
                    }
                    if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.amount))
                    {
                        this.Discount = Convert.ToDecimal(DiscountOnRenewalSettings.value);
                    }
                    break;
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                    this.AnnualRiskPremium = premium + discountField;
                    if (isVehicleRegisteredonICEcash && !(coverType == eCoverType.Comprehensive))
                    {
                        this.AnnualRiskPremium = Convert.ToDecimal(BasicPremiumICEcash);
                    }
                    if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                    {
                        this.Discount = ((this.AnnualRiskPremium * Convert.ToDecimal(DiscountOnRenewalSettings.value)) / 100);
                    }
                    if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.amount))
                    {
                        this.Discount = Convert.ToDecimal(DiscountOnRenewalSettings.value);
                    }
                    break;
                    
            }

            // totalPremium = premium - this.Discount;
            decimal totalPremium = 0;

            if (coverType == eCoverType.Comprehensive)
            {
                totalPremium = (this.Premium + discountField) - this.Discount;

            }
            else
            {
                totalPremium = ((isVehicleRegisteredonICEcash ? Convert.ToDecimal(BasicPremiumICEcash) : this.Premium)+ discountField) - this.Discount;
            }



            //  premium = (decimal)totalPremium;


            this.Premium = Math.Round(totalPremium, 2);


            var stampDuty = 0.00m;
            if (StampDutySetting.ValueType == Convert.ToInt32(eSettingValueType.percentage))
            {
                stampDuty = (totalPremium * Convert.ToDecimal(StampDutySetting.value)) / 100;
            }
            else
            {
                stampDuty = totalPremium + Convert.ToDecimal(StampDutySetting.value);
            }


            //if (stampDuty > 2000000)
            //{
            //    Status = false;
            //    this.Message = "Stamp Duty must not exceed $2,000,000";
            //}


            var ztscLevy = 0.00m;

            decimal totalPremiumForZtscLevy = 0;

            //if (isVehicleRegisteredonICEcash && !(coverType == eCoverType.Comprehensive))
            //{
            //    totalPremiumForZtscLevy = (isVehicleRegisteredonICEcash ? Convert.ToDecimal(BasicPremiumICEcash) : this.Premium) + this.PassengerAccidentCoverAmount + this.RoadsideAssistanceAmount + this.MedicalExpensesAmount + this.ExcessBuyBackAmount + this.ExcessAmount;
            //}
            //else
            //{
            //    totalPremiumForZtscLevy = (this.Premium) + this.PassengerAccidentCoverAmount + this.RoadsideAssistanceAmount + this.MedicalExpensesAmount + this.ExcessBuyBackAmount + this.ExcessAmount;
            //}


            if (coverType == eCoverType.Comprehensive)
            {
                totalPremiumForZtscLevy = (this.Premium + discountField) ;

            }
            else
            {
                totalPremiumForZtscLevy = (isVehicleRegisteredonICEcash ? Convert.ToDecimal(BasicPremiumICEcash) : this.Premium) + discountField;

            }


            if (ZTSCLevySetting.ValueType == Convert.ToInt32(eSettingValueType.percentage))
            {
                // ztscLevy = (totalPremium * Convert.ToDecimal(ZTSCLevySetting.value)) / 100;
                ztscLevy = (totalPremiumForZtscLevy * Convert.ToDecimal(ZTSCLevySetting.value)) / 100;
            }
            else
            {
                // ztscLevy = totalPremium + Convert.ToDecimal(ZTSCLevySetting.value);
                ztscLevy = totalPremiumForZtscLevy + Convert.ToDecimal(ZTSCLevySetting.value);
            }


            this.StamDuty = Math.Round(stampDuty, 2);
            this.ZtscLevy = Math.Round(ztscLevy, 2);

            if (isVehicleRegisteredonICEcash && !(coverType == eCoverType.Comprehensive) && totalPremium == Convert.ToDecimal(BasicPremiumICEcash))
            {
                this.StamDuty = Math.Round(Convert.ToDecimal(StampDutyICEcash), 2);
                this.ZtscLevy = Math.Round(Convert.ToDecimal(ZTSCLevyICEcash), 2);
            }


            if (!string.IsNullOrEmpty(StampDutyICEcash) && Convert.ToDecimal(StampDutyICEcash) > 100000)
            {
                this.StamDuty = 100000;
            }

            // if product "Private car"




            if (ProductId == 1)
            {

                if (PaymentTermid == 1)
                {
                    if (Convert.ToDouble(this.ZtscLevy) > 10.80)
                    {
                        double maxZTSC = 10.80;
                        this.ZtscLevy = Math.Round(Convert.ToDecimal(maxZTSC), 2);
                    }
                }
                else if (PaymentTermid == 4)
                {
                    double maxZTSC = 10.80 / 3;

                    if (Convert.ToDouble(this.ZtscLevy) > maxZTSC)
                    {

                        this.ZtscLevy = Math.Round(Convert.ToDecimal(maxZTSC), 2);
                    }
                }
            }

            if (ProductId == 3)
            {

                if (PaymentTermid == 1)
                {
                    if (Convert.ToDouble(this.ZtscLevy) > 22.00)
                    {
                        double maxZTSC = 22.00;
                        this.ZtscLevy = Math.Round(Convert.ToDecimal(maxZTSC), 2);
                    }
                }
                else if (PaymentTermid == 4)
                {
                    double maxZTSC = 22.00 / 3;

                    if (Convert.ToDouble(this.ZtscLevy) > maxZTSC)
                    {

                        this.ZtscLevy = Math.Round(Convert.ToDecimal(maxZTSC), 2);
                    }
                }

            }



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
