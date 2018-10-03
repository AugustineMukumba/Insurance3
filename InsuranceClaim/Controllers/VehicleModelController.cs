using AutoMapper;
using Insurance.Domain;
using InsuranceClaim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InsuranceClaim.Controllers
{
    public class VehicleModelController : Controller
    {
        // GET: VehicleModel
        public ActionResult Index()
        {

            InsuranceClaim.Models.ClsVehicleModel obj = new InsuranceClaim.Models.ClsVehicleModel();
            List<Insurance.Domain.VehicleModel> objList = new List<Insurance.Domain.VehicleModel>();
            objList = InsuranceContext.VehicleModels.All().ToList();
            ViewBag.MakeList = MakeList();
            return View();
        }

        public List<Insurance.Domain.VehicleMake> MakeList()
        {
            return InsuranceContext.VehicleMakes.All().ToList();
        }

        [HttpPost]
        public ActionResult SaveVehicleModel(ClsVehicleModel model)
        {
            var dbModel = Mapper.Map<ClsVehicleModel, VehicleModel>(model);
            dbModel.ModelDescription = model.ModelDescription.ToUpper();
            dbModel.CreatedOn = DateTime.Now;
            dbModel.ModifiedOn = DateTime.Now;
            InsuranceContext.VehicleModels.Insert(dbModel);
            return RedirectToAction("VehicleModelList");
        }
        public ActionResult VehicleModelList()
        {
            var modellist = InsuranceContext.VehicleModels.All(where: "IsActive= 'True' or IsActive is Null").OrderByDescending(x=>x.Id).ToList();
            return View(modellist);
        }

        public ActionResult VehicleModelEdit(int Id)
        {
            var record = InsuranceContext.VehicleModels.All(where: $"Id ={Id}").FirstOrDefault();
            var model = Mapper.Map<VehicleModel, ClsVehicleModel>(record);
            ViewBag.MakeList = MakeList();

            return View(model);
        }

        [HttpPost]
        public ActionResult VehicleModelEdit(VehicleModel model)
        {
            if (ModelState.IsValid)
            {
                var modelid = model.Id;
                //var data = Mapper.Map<VehiclesMakeModel, VehicleMake>(model);
                var data = InsuranceContext.VehicleModels.Single(where: $"Id = {modelid}");
                data.ModelDescription = model.ModelDescription.ToUpper();
                data.MakeCode = model.MakeCode;
                data.ShortDescription = model.ShortDescription;
                data.ModelCode = model.ModelCode;
                //data.CreatedOn = model.CreatedOn;
                data.ModifiedOn = DateTime.Now;
                InsuranceContext.VehicleModels.Update(data);

            }
            return RedirectToAction("VehicleModelList");
        }
        public ActionResult DeleteModel(int id)
        {
            string query = $"update VehicleModel set IsActive = 0 where Id={id}";
            InsuranceContext.VehicleModels.Execute(query);

            return RedirectToAction("VehicleModelList");
        }
    }
}
