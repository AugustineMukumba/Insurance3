using Insurance.Domain;
using InsuranceClaim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insurance.Service
{
    public static class MiscellaneousService
    {
        public static string GetCustomerNamebyID(int id)
        {
            var list = InsuranceContext.Customers.Single(id);
            if (list != null)
            {
                return list.FirstName + " " + list.LastName;
            }
            return "";

        }

        public static string GetPaymentMethodNamebyID(int id)
        {
            var list = InsuranceContext.PaymentMethods.Single(id);
            if (list != null)
            {
                return list.Name;
            }
            return "";

        }

        public static string GetCoverTypeNamebyID(int id)
        {
            var list = InsuranceContext.CoverTypes.Single(id);
            if (list != null)
            {
                return list.Name;
            }
            return "";

        }

        public static string GetMakeNamebyMakeCode(string code)
        {
            var list = InsuranceContext.VehicleMakes.Single(where: $"MakeCode='{code}'");
            if (list != null)
            {
                return list.MakeDescription;
            }
            return "";

        }

        public static string GetModelNamebyModelCode(string code)
        {
            var list = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{code}'");
            if (list != null)
            {
                return list.ModelDescription;
            }
            return "";

        }

        public static string GetReinsuranceBrokerNamebybrokerid(int id)
        {
            var list = InsuranceContext.ReinsuranceBrokers.Single(id);
            if (list != null)
            {
                return list.ReinsuranceBrokerName;
            }
            return "";

        }

        public static string AddLoyaltyPoints(int CustomerId, int PolicyId, RiskDetailModel vehicle)
        {
            var loaltyPointsSettings = InsuranceContext.Settings.Single(where: $"keyname='Points On Renewal'");
            var loyaltyPoint = 0.00m;
            switch (vehicle.PaymentTermId)
            {
                case 1:                    
                    if (loaltyPointsSettings.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                    {
                        loyaltyPoint = ((Convert.ToDecimal(vehicle.AnnualRiskPremium) * Convert.ToDecimal(loaltyPointsSettings.value)) / 100);
                    }
                    if (loaltyPointsSettings.ValueType == Convert.ToInt32(eSettingValueType.amount))
                    {
                        loyaltyPoint = Convert.ToDecimal(loaltyPointsSettings.value);
                    }
                    break;
                case 3:
                    if (loaltyPointsSettings.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                    {
                        loyaltyPoint = ((Convert.ToDecimal(vehicle.QuaterlyRiskPremium) * Convert.ToDecimal(loaltyPointsSettings.value)) / 100);
                    }
                    if (loaltyPointsSettings.ValueType == Convert.ToInt32(eSettingValueType.amount))
                    {
                        loyaltyPoint = Convert.ToDecimal(loaltyPointsSettings.value);
                    }
                    break;
                case 4:
                    if (loaltyPointsSettings.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                    {
                        loyaltyPoint = ((Convert.ToDecimal(vehicle.TermlyRiskPremium) * Convert.ToDecimal(loaltyPointsSettings.value)) / 100);
                    }
                    if (loaltyPointsSettings.ValueType == Convert.ToInt32(eSettingValueType.amount))
                    {
                        loyaltyPoint = Convert.ToDecimal(loaltyPointsSettings.value);
                    }
                    break;
            }

            LoyaltyDetail objLoyaltydetails = new LoyaltyDetail();
            objLoyaltydetails.CustomerId = CustomerId;
            objLoyaltydetails.IsActive = true;
            objLoyaltydetails.PolicyId = PolicyId;
            objLoyaltydetails.PointsEarned = loyaltyPoint;
            objLoyaltydetails.CreatedBy = CustomerId;
            objLoyaltydetails.CreatedOn = DateTime.Now;

            InsuranceContext.LoyaltyDetails.Insert(objLoyaltydetails);

            return "";
        }
    }
}
