using Insurance.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using InsuranceClaim.Models;

namespace Insurance.Service
{
    public class VehicleService
    {
        public List<VehicleMake> GetMakers()
        {
            var list = InsuranceContext.VehicleMakes.All().ToList();
            return list;
        }

        public List<ClsVehicleModel> GetModel(string makeCode)
        {
            var list = InsuranceContext.VehicleModels.All(where: $"MakeCode='{makeCode}'").ToList();
            var map = Mapper.Map<List<VehicleModel>, List<ClsVehicleModel>>(list);
            return map;

        }
        public List<CoverType> GetCoverType()
        {
            var list = InsuranceContext.CoverTypes.All().ToList();
            return list;
        }
        public List<AgentCommission> GetAgentCommission()
        {
            var list = InsuranceContext.AgentCommissions.All().ToList();
            return list;
        }
        public List<VehicleUsage> GetVehicleUsage(int policyId)
        {
            var policy = InsuranceContext.PolicyDetails.Single(policyId);
            var list = InsuranceContext.VehicleUsages.All(where: $"ProductId='{policy.PolicyName}'").ToList();
            return list;
        }
    }
}
