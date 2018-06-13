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
    }
}
