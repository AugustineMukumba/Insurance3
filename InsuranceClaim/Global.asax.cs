using AutoMapper;
using Insurance.Domain;
using Insurance.Domain.Code;
using InsuranceClaim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace InsuranceClaim
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            RunMgr.Instance.Init();
            Map();
           
        }
        private void Map()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Customer, CustomerModel>().ReverseMap();
                cfg.CreateMap<PolicyDetailModel, PolicyDetail>().ReverseMap();
                cfg.CreateMap<RiskDetailModel, VehicleDetail>().ReverseMap();
                cfg.CreateMap<VehicleModel, ClsVehicleModel>().ReverseMap();
                cfg.CreateMap<InsurerModel, PolicyInsurer>().ReverseMap();
            });
        }
    }
}
