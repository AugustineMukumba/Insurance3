using Insurance.Domain;
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
    }
}
