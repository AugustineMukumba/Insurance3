using Insurance.Domain;
using InsuranceClaim.Models;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Insurance.Service
{
    public static class MiscellaneousService
    {
        public static void UpdateBalanceForVehicles(decimal amountPaid, int SummaryID, decimal totalPremium, bool isRenew, int renewVehicleID = 0)
        {
            List<SummaryVehicleDetail> _SummaryVehicleDetails = new List<SummaryVehicleDetail>();

            _SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={SummaryID}").ToList();

            if (amountPaid <= totalPremium)
            {
                if (isRenew)
                {

                }
                else
                {
                    var balanceFromAmountPaid = amountPaid;
                    var listVehicles = new List<VehicleDetail>();
                    foreach (var item in _SummaryVehicleDetails)
                    {
                        var vehicle = InsuranceContext.VehicleDetails.Single(item.VehicleDetailsId);
                        if (vehicle != null)
                        {
                            listVehicles.Add(vehicle);
                        }

                    }

                    if (listVehicles != null && listVehicles.Count > 0)
                    {
                        listVehicles = listVehicles.OrderBy(x => x.Premium).ToList();

                        foreach (var _item in listVehicles)
                        {

                            var vehicletotalPremium = _item.Premium + _item.StampDuty + _item.ZTSCLevy + (Convert.ToBoolean(_item.IncludeRadioLicenseCost) ? _item.RadioLicenseCost : 0.00m);
                            if (balanceFromAmountPaid > 0.00m)
                            {
                                if (balanceFromAmountPaid >= vehicletotalPremium)
                                {
                                    balanceFromAmountPaid = Convert.ToDecimal(balanceFromAmountPaid - vehicletotalPremium);
                                    _item.BalanceAmount = 0.00m;
                                }
                                else
                                {
                                    _item.BalanceAmount = vehicletotalPremium - balanceFromAmountPaid;
                                    balanceFromAmountPaid = 0.00m;
                                }
                            }
                            else
                            {
                                _item.BalanceAmount = vehicletotalPremium;
                            }

                            InsuranceContext.VehicleDetails.Update(_item);
                        }
                    }

                }
            }
        }

        public static string EmailPdf(string MotorBody, int custid, string policynumber, string filename, int vehcleId = 0)
        {
            StringReader sr = new StringReader(MotorBody.ToString());

            string path = "";

            try
            {

                Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
                string vehiclefolderpath = "";
               



                using (MemoryStream memoryStream = new MemoryStream())
                {
                    PdfWriter writer = PdfWriter.GetInstance(pdfDoc, memoryStream);
                    pdfDoc.Open();
                    htmlparser.Parse(sr);
                    pdfDoc.Close();
                    byte[] bytes = memoryStream.ToArray();
                    memoryStream.Close();

                    string custfolderpath = HttpContext.Current.Server.MapPath("~/Documents/" + custid + "/");
                    string policyfolderpath = HttpContext.Current.Server.MapPath("~/Documents/" + custid + "/" + policynumber + "/");

                    if (vehcleId > 0)
                    {
                        vehiclefolderpath = HttpContext.Current.Server.MapPath("~/Documents/" + custid + "/" + policynumber + "/" + vehcleId + "/");
                    }


                    if (!Directory.Exists(custfolderpath))
                    {
                        Directory.CreateDirectory(custfolderpath);
                        Directory.CreateDirectory(policyfolderpath);
                    }
                    else
                    {
                        if (!Directory.Exists(policyfolderpath))
                        {
                            Directory.CreateDirectory(policyfolderpath);
                            if (vehcleId > 0)
                            {
                                Directory.CreateDirectory(vehiclefolderpath);
                            }


                        }
                        else
                        {
                            if (vehcleId > 0)
                            {
                                if (!Directory.Exists(vehiclefolderpath))
                                {
                                    Directory.CreateDirectory(vehiclefolderpath);
                                }
                            }

                        }

                    }
                    if (vehcleId > 0)
                    {

                        System.IO.File.WriteAllBytes(vehiclefolderpath + filename + ".pdf", memoryStream.ToArray());
                        path = "~/Documents/" + custid + "/" + policynumber + "/" + vehcleId + "/" + filename + ".pdf";

                    }
                    else
                    {
                        System.IO.File.WriteAllBytes(policyfolderpath + filename + ".pdf", memoryStream.ToArray());

                        //    path = "http://" + HttpContext.Current.Request.Url.Authority + "/" + "~/Documents/" + custid + "/" + policynumber + "/" + filename + ".pdf";


                        path = "~/Documents/" + custid + "/" + policynumber + "/" + filename + ".pdf";


                    }



                }

            }
            catch (Exception ex)
            {

            }

            sr.Close();

            return path;

        }

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

            Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();
            var policy = InsuranceContext.PolicyDetails.Single(PolicyId);
            var customer = InsuranceContext.Customers.Single(CustomerId);
            var TotalLoyaltyPoints = InsuranceContext.LoyaltyDetails.All(where: $"CustomerId={CustomerId}").Sum(x => x.PointsEarned);

            string ReminderEmailPath = "/Views/Shared/EmaiTemplates/LoyaltyPoints.cshtml";
            string EmailBody2 = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(ReminderEmailPath));
            var body = EmailBody2.Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##CreditedWalletAmount##", Convert.ToString(loyaltyPoint)).Replace("##TotalWalletBalance##", Convert.ToString(TotalLoyaltyPoints));
          
           // var yAtter = "E:/Deepak/insuranceClaim10-9-2018/InsuranceClaim/Pdf";
            var attacheMentPath = MiscellaneousService.EmailPdf(body, policy.CustomerId, policy.PolicyNumber, "Loyalty Points");

            List<string> attachements = new List<string>();
            attachements.Add(attacheMentPath);
            


       objEmailService.SendEmail(HttpContext.Current.User.Identity.Name, "", "", "Loyalty Reward | Points Credited to your Wallet", body, attachements);


            return "";
        }
    }
}
