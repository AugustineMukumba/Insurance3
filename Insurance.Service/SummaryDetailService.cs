using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Insurance.Domain;
using AutoMapper;
namespace Insurance.Service
{
    public class SummaryDetailService
    {
        public VehicleDetail GetVehicleInformation(int vehicleId)
        {
            var vehicle = InsuranceContext.VehicleDetails.Single(vehicleId);
            return vehicle;
        }

        public Int32 getNewDebitNote()
        {
            var vehicle = InsuranceContext.SummaryDetails.Max("id");

            if (vehicle != null)
            {
                return Convert.ToInt32(vehicle) + 1;
            }
            else
            {
                return 1;
            }

        }


        public List<Currency> GetAllCurrency()
        {
            return InsuranceContext.Currencies.All().ToList();
        }


        public string GetCurrencyName(List<Currency> currenyList, int? currencyId)
        {

    
            var currencyDetails = currenyList.FirstOrDefault(c => c.Id == currencyId);
            if (currencyDetails != null)
                return currencyDetails.Name;
            else
                return  "USD";
        }





    }
}
