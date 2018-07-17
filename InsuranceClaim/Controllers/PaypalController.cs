﻿using InsuranceClaim.Models;
using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Insurance.Domain;
using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace InsuranceClaim.Controllers
{
    public class PaypalController : Controller
    {
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        //
        // GET: /Paypal/

        public ActionResult Index(int id)
        {

            return View();
        }

        public ActionResult PaymentWithCreditCard(CardDetailModel model)
        {
            Session["CardDetail"] = model;
            //create and item for which you are taking payment
            //if you need to add more items in the list
            //Then you will need to create multiple item objects or use some loop to instantiate object
            var summaryDetail = InsuranceContext.SummaryDetails.Single(model.SummaryDetailId);
            var vehicle = InsuranceContext.VehicleDetails.Single(summaryDetail.VehicleDetailId);
            var policy = InsuranceContext.PolicyDetails.Single(vehicle.PolicyId);
            var customer = InsuranceContext.Customers.Single(summaryDetail.CustomerId);
            //var summaryDetail = (SummaryDetailModel)Session["SummaryDetailed"];
            //var vehicle = (RiskDetailModel)Session["VehicleDetail"];
            //var policy = (PolicyDetail)Session["PolicyData"];
            //var customer = (CustomerModel)Session["CustomerDataModal"];
            var product = InsuranceContext.Products.Single(Convert.ToInt32(policy.PolicyName));
            var currency = InsuranceContext.Currencies.Single(policy.CurrencyId);

            //var paymentInformations = InsuranceContext.PaymentInformations.SingleCustome(model.SummaryDetailId);

            double totalPremium = Convert.ToDouble(summaryDetail.TotalPremium);
            var term = summaryDetail.PaymentTermId;
            if (term == Convert.ToInt32(ePaymentTerm.Monthly))
            {
                totalPremium = Math.Round(totalPremium / 12, 2);
            }
            else if (term == Convert.ToInt32(ePaymentTerm.Quarterly))
            {
                totalPremium = Math.Round(totalPremium / 4, 2);
            }
            else if (term == Convert.ToInt32(ePaymentTerm.Termly))
            {
                totalPremium = Math.Round(totalPremium / 3, 2);
            }
            //check if single decimal place
            string zeros = string.Empty;
            try
            {
                var percision = totalPremium.ToString().Split('.');
                var length = 2 - percision[1].Length;
                for (int i = 0; i < length; i++)
                {
                    zeros += "0";
                }
            }
            catch
            {
                zeros = ".00";

            }


            //Item item = new Item();
            //item.name = product.ProductName;            
            //item.currency = currency.Name;
            //item.price = totalPremium.ToString() + zeros;
            //item.quantity = "1";
            //item.sku = "sku";

            ////Now make a List of Item and add the above item to it
            ////you can create as many items as you want and add to this list
            //List<Item> itms = new List<Item>();
            //itms.Add(item);
            //ItemList itemList = new ItemList();
            //itemList.items = itms;

            ////Address for the payment
            //Address billingAddress = new Address();
            //billingAddress.city = "NewYork";
            //billingAddress.country_code = "US";
            //billingAddress.line1 = customer.AddressLine1 == string.Empty ? customer.AddressLine2 : customer.AddressLine1;
            //billingAddress.postal_code = "43210";
            //billingAddress.state = "NY";


            ////Now Create an object of credit card and add above details to it
            //PayPal.Api.CreditCard crdtCard = new PayPal.Api.CreditCard();
            //crdtCard.billing_address = billingAddress;
            //crdtCard.cvv2 = model.CVC;
            //crdtCard.expire_month = Convert.ToInt32(model.ExpiryDate.Split('/')[0]);
            //crdtCard.expire_year = Convert.ToInt32(model.ExpiryDate.Split('/')[1]);
            //crdtCard.first_name = model.NameOnCard;
            ////crdtCard.last_name = "Thakur";
            //crdtCard.number = model.CardNumber;
            //crdtCard.type = CreditCardUtility.GetTypeName(model.CardNumber).ToLower();

            //// Specify details of your payment amount.
            //Details details = new Details();
            //details.shipping = "0.00";
            //details.subtotal = totalPremium.ToString() + zeros;
            //details.tax = "0.00";

            //// Specify your total payment amount and assign the details object
            //Amount amnt = new Amount();
            //amnt.currency = currency.Name;
            //// Total = shipping tax + subtotal.
            //amnt.total = totalPremium.ToString() + zeros;            
            //// amnt.details = details;

            //// Now make a trasaction object and assign the Amount object
            //Transaction tran = new Transaction();
            //tran.amount = amnt;
            //tran.description = "Online vehicle insurance purchased";
            //tran.item_list = itemList;
            //tran.invoice_number = summaryDetail.DebitNote;

            //// Now, we have to make a list of trasaction and add the trasactions object
            //// to this list. You can create one or more object as per your requirements

            //List<Transaction> transactions = new List<Transaction>();
            //transactions.Add(tran);

            //// Now we need to specify the FundingInstrument of the Payer
            //// for credit card payments, set the CreditCard which we made above

            //FundingInstrument fundInstrument = new FundingInstrument();
            //fundInstrument.credit_card = crdtCard;

            //// The Payment creation API requires a list of FundingIntrument

            //List<FundingInstrument> fundingInstrumentList = new List<FundingInstrument>();
            //fundingInstrumentList.Add(fundInstrument);

            //PayerInfo pi = new PayerInfo();
            //pi.email = "noemail@noemail.com";
            //pi.first_name = "Stack";
            //pi.last_name = "Overflow";
            //pi.shipping_address = new ShippingAddress
            //{
            //    city = "San Mateo",
            //    country_code = "US",
            //    line1 = "SO TEST",
            //    line2 = "",
            //    postal_code = "94002",
            //    state = "CA",
            //};

            //// Now create Payer object and assign the fundinginstrument list to the object
            //Payer payr = new Payer();
            //payr.funding_instruments = fundingInstrumentList;
            //payr.payment_method = "credit_card";
            //payr.payer_info = pi;

            //// finally create the payment object and assign the payer object & transaction list to it
            //Payment pymnt = new Payment();
            //pymnt.intent = "sale";
            //pymnt.payer = payr;
            //pymnt.transactions = transactions;




            Item item = new Item();
            item.name = product.ProductName;
            item.currency = "USD";
            item.price = totalPremium.ToString() + zeros;
            item.quantity = "1";
            item.sku = "sku";
            //item.currency = "USD";

            Session["itemData"] = item;
            List<Item> itms = new List<Item>();
            itms.Add(item);
            ItemList itemList = new ItemList();
            itemList.items = itms;

            Address billingAddress = new Address();
            billingAddress.city = customer.City;
            billingAddress.country_code = "US";
            billingAddress.line1 = customer.AddressLine1 == string.Empty ? customer.AddressLine2 : customer.AddressLine1;
            billingAddress.line2 = customer.AddressLine2 == string.Empty ? customer.AddressLine1 : customer.AddressLine2;

            if (customer.Zipcode == null)
            {
                billingAddress.postal_code = "00263";
            }
            else
            {
                billingAddress.postal_code = customer.Zipcode;
            }

            billingAddress.state = customer.State;

            PayPal.Api.CreditCard crdtCard = new PayPal.Api.CreditCard();
            crdtCard.billing_address = billingAddress;
            crdtCard.cvv2 = model.CVC;
            crdtCard.expire_month = Convert.ToInt32(model.ExpiryDate.Split('/')[0]);
            crdtCard.expire_year = Convert.ToInt32(model.ExpiryDate.Split('/')[1]);

            //crdtCard.first_name = "fgdfg";
            //crdtCard.last_name = "rffd";

            var name = model.NameOnCard.Split(' ');
            if (name.Length == 1)
            {
                crdtCard.first_name = name[0];
                crdtCard.last_name = null;
            }
            if (name.Length == 2)
            {
                crdtCard.first_name = name[0];
                crdtCard.last_name = name[1];
            }

            crdtCard.number = model.CardNumber; //use some other test number if it fails
            crdtCard.type = CreditCardUtility.GetTypeName(model.CardNumber).ToLower();

            Details details = new Details();
            details.tax = "0";
            details.shipping = "0";
            details.subtotal = totalPremium.ToString() + zeros;

            Amount amont = new Amount();
            amont.currency = "USD";
            amont.total = totalPremium.ToString() + zeros;
            amont.details = details;

            Transaction tran = new Transaction();
            tran.amount = amont;
            tran.description = "trnx desc";
            tran.item_list = itemList;

            List<Transaction> transactions = new List<Transaction>();
            transactions.Add(tran);

            FundingInstrument fundInstrument = new FundingInstrument();
            fundInstrument.credit_card = crdtCard;

            List<FundingInstrument> fundingInstrumentList = new List<FundingInstrument>();
            fundingInstrumentList.Add(fundInstrument);



            var User = UserManager.FindById(customer.UserID);
            PayerInfo pi = new PayerInfo();
            pi.email = User.Email;
            pi.first_name = customer.FirstName;
            pi.last_name = customer.LastName;
            pi.shipping_address = new ShippingAddress
            {
                city = customer.City,
                country_code = "US",
                line1 = customer.AddressLine1 == string.Empty ? customer.AddressLine2 : customer.AddressLine1,
                line2 = customer.AddressLine2 == string.Empty ? customer.AddressLine1 : customer.AddressLine2,
                postal_code = customer.Zipcode,
                state = customer.State,
            };



            Payer payr = new Payer();
            payr.funding_instruments = fundingInstrumentList;
            payr.payment_method = "credit_card";
            payr.payer_info = pi;

            Payment pymnt = new Payment();
            pymnt.intent = "sale";
            pymnt.payer = payr;
            pymnt.transactions = transactions;

            try
            {
                //getting context from the paypal, basically we are sending the clientID and clientSecret key in this function 
                //to the get the context from the paypal API to make the payment for which we have created the object above.

                //Code for the configuration class is provided next

                // Basically, apiContext has a accesstoken which is sent by the paypal to authenticate the payment to facilitator account. An access token could be an alphanumeric string

                APIContext apiContext = Configuration.GetAPIContext();

                // Create is a Payment class function which actually sends the payment details to the paypal API for the payment. The function is passed with the ApiContext which we received above.

                Payment createdPayment = pymnt.Create(apiContext);
                //paymentInformations.PaymentTransctionId = createdPayment.id;
                Session["PaymentId"] = createdPayment.id;

                //if the createdPayment.State is "approved" it means the payment was successfull else not
                creatInvoice(User, customer);
                if (createdPayment.state.ToLower() != "approved")
                {
                    ModelState.AddModelError("PaymentError", "Payment not approved");
                    return RedirectToAction("PaymentDetail", "CustomerRegistration", new { id = model.SummaryDetailId });
                }
            }
            catch (PayPal.PayPalException ex)
            {
                Logger.Log("Error: " + ex.Message);
                ModelState.AddModelError("PaymentError", ex.Message);
                TempData["ErrorMessage"] = ex.Message;
                JavaScriptSerializer json_serializer = new JavaScriptSerializer();

                var error = json_serializer.DeserializeObject(((PayPal.ConnectionException)ex).Response);
                return RedirectToAction("PaymentDetail", "CustomerRegistration", new { id = model.SummaryDetailId });
            }

            return RedirectToAction("SaveDetailList", "Paypal", new { id = model.SummaryDetailId });
        }
        class Test
        {

            String test;

            String getTest() { return test; }
            void setTest(String test) { this.test = test; }

        }
        private ActionResult creatInvoice(ApplicationUser User, Customer customer)
        {
            APIContext apiContext = Configuration.GetAPIContext();

            var data = (Item)Session["itemData"];

            var invoice = new Invoice()
            {

                merchant_info = new MerchantInfo()
                {
                    email = "ankit.dhiman-facilitator@kindlebit.com",
                    first_name = "Genetic Financial Services",
                    last_name = "11 Routledge Street Milton Park",
                    business_name = "Insurance Claim",
                    website = "insuranceclaim.com",
                    //tax_id = "47-4559942",

                    phone = new Phone()
                    {
                        country_code = "001",
                        national_number = "08677007491"
                    },
                    address = new InvoiceAddress()
                    {
                        line1 = customer.AddressLine1,
                        city = customer.AddressLine2,
                        state = customer.City + ", " + customer.State,
                        postal_code = customer.Zipcode,
                        country_code = "US"

                    }
                },

                billing_info = new List<BillingInfo>()
                            {
                                new BillingInfo()
                                {

                                    email = User.Email,//"amit.kamal@kindlebit.com",
                                    first_name=customer.FirstName,
                                    last_name=customer.LastName
                                }
                            },

                items = new List<InvoiceItem>()
                            {
                                new InvoiceItem()
                                {
                                    name = data.name,
                                    quantity = 1,
                                    unit_price = new PayPal.Api.Currency()
                                    {
                                        currency = "USD",
                                        value =data.price

                                    },
                                },
                            },
                note = "Your  Invoce has been created successfully.",

                shipping_info = new ShippingInfo()
                {
                    first_name = customer.FirstName,
                    last_name = customer.LastName,
                    business_name = "InsuranceClaim",
                    address = new InvoiceAddress()
                    {
                        //line1 = userdata.State.ToString(),
                        city = customer.City,
                        state = customer.State,
                        postal_code = customer.Zipcode,
                        country_code = "US"
                    }
                }
            };
            var createdInvoice = invoice.Create(apiContext);
            Session["InvoiceId"] = createdInvoice.id;
            createdInvoice.Send(apiContext);

            return null;
        }
        public ActionResult PaymentWithPaypal()
        {
            //getting the apiContext as earlier
            APIContext apiContext = Configuration.GetAPIContext();

            try
            {
                string payerId = Request.Params["PayerID"];

                if (string.IsNullOrEmpty(payerId))
                {
                    //this section will be executed first because PayerID doesn't exist

                    //it is returned by the create function call of the payment class

                    // Creating a payment

                    // baseURL is the url on which paypal sendsback the data.

                    // So we have provided URL of this controller only

                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Paypal/PaymentWithPayPal?";

                    //guid we are generating for storing the paymentID received in session

                    //after calling the create function and it is used in the payment execution

                    var guid = Convert.ToString((new Random()).Next(100000));

                    //CreatePayment function gives us the payment approval url

                    //on which payer is redirected for paypal acccount payment

                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid);

                    //get links returned from paypal in response to Create function call

                    var links = createdPayment.links.GetEnumerator();

                    string paypalRedirectUrl = null;

                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;

                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            //saving the payapalredirect URL to which user will be redirected for payment
                            paypalRedirectUrl = lnk.href;
                        }
                    }

                    // saving the paymentID in the key guid
                    Session.Add(guid, createdPayment.id);

                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    // This section is executed when we have received all the payments parameters

                    // from the previous call to the function Create

                    // Executing a payment

                    var guid = Request.Params["guid"];

                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);

                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View("FailureView");
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error" + ex.Message);
                return View("FailureView");
            }

            return View("SuccessView");
        }

        private PayPal.Api.Payment payment;

        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution() { payer_id = payerId };
            this.payment = new Payment() { id = paymentId };
            return this.payment.Execute(apiContext, paymentExecution);
        }

        private Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {

            //similar to credit card create itemlist and add item objects to it
            var itemList = new ItemList() { items = new List<Item>() };

            itemList.items.Add(new Item()
            {
                name = "Item Name",
                currency = "USD",
                price = "5",
                quantity = "1",
                sku = "sku"
            });

            var payer = new Payer() { payment_method = "paypal" };

            // Configure Redirect Urls here with RedirectUrls object
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl,
                return_url = redirectUrl
            };

            // similar as we did for credit card, do here and create details object
            var details = new Details()
            {
                tax = "1",
                shipping = "1",
                subtotal = "5"
            };

            // similar as we did for credit card, do here and create amount object
            var amount = new Amount()
            {
                currency = "USD",
                total = "7", // Total must be equal to sum of shipping, tax and subtotal.
                details = details
            };

            var transactionList = new List<Transaction>();

            transactionList.Add(new Transaction()
            {
                description = "Transaction description.",
                invoice_number = "your invoice number",
                amount = amount,
                item_list = itemList
            });

            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };

            // Create a payment using a APIContext
            return this.payment.Create(apiContext);

        }
        public async Task<ActionResult> SaveDetailList(Int32 id)
        {
            var PaymentId = Session["PaymentId"];
            var InvoiceId = Session["InvoiceId"];
            var summaryDetail = InsuranceContext.SummaryDetails.Single(id);
            var vehicle = InsuranceContext.VehicleDetails.Single(summaryDetail.VehicleDetailId);
            var policy = InsuranceContext.PolicyDetails.Single(vehicle.PolicyId);
            var customer = InsuranceContext.Customers.Single(summaryDetail.CustomerId);
            var product = InsuranceContext.Products.Single(Convert.ToInt32(policy.PolicyName));
            var currency = InsuranceContext.Currencies.Single(policy.CurrencyId);
            var paymentInformations = InsuranceContext.PaymentInformations.SingleCustome(id);
            var user = UserManager.FindById(customer.UserID);
            //var user = Microsoft.AspNet.Identity.GetUserManager<ApplicationUserManager>();
            var DebitNote = summaryDetail.DebitNote;
            PaymentInformation objSaveDetailListModel = new PaymentInformation();
            objSaveDetailListModel.CurrencyId = policy.CurrencyId;
            objSaveDetailListModel.PolicyId = vehicle.PolicyId;
            objSaveDetailListModel.VehicleDetailId = summaryDetail.VehicleDetailId.Value;
            objSaveDetailListModel.CustomerId = summaryDetail.CustomerId.Value;
            objSaveDetailListModel.SummaryDetailId = id;
            objSaveDetailListModel.DebitNote = summaryDetail.DebitNote;
            objSaveDetailListModel.ProductId = product.Id;
            objSaveDetailListModel.PaymentId = PaymentId == null ? "" : PaymentId.ToString();
            objSaveDetailListModel.InvoiceId = InvoiceId == null ? "" : InvoiceId.ToString();

            // Insurance.Service.PaymentInformationService objPaymentInformationService = new Insurance.Service.PaymentInformationService();
            //var isExist= objPaymentInformationService.GetById(id);
            // if (true)
            // {

            // }
            if (paymentInformations == null)
            {
                Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;


                if (!userLoggedin)
                {
                    InsuranceContext.PaymentInformations.Insert(objSaveDetailListModel);
                    string emailTemplatePath = "/Views/Shared/EmaiTemplates/UserRegisteration.cshtml";
                    string EmailBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(emailTemplatePath));
                    var Body = EmailBody.Replace(" #PolicyNumber#", policy.PolicyNumber).Replace("#TodayDate#", DateTime.Now.ToShortDateString()).Replace("#FirstName#", customer.FirstName).Replace("#LastName#", customer.LastName).Replace("#Address1#", customer.AddressLine1).Replace("#Address2#", customer.AddressLine2).Replace("#Email#", user.Email).Replace("#change#", callbackUrl);
                    objEmailService.SendEmail(user.Email, "", "", "Account Creation", Body, null);



                    Insurance.Service.smsService objsmsService = new Insurance.Service.smsService();

                    string body = "Hello " + customer.FirstName + "\nWelcome to the GENE-INSURE family, we would like to simplify your life." + "\nYour policy number is : " + policy.PolicyNumber + "\nUsername is : " + user.Email + "\nYour Password : Geneinsure@123" + "\nPlease reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>" + "\nThank you once again.";
                    var result = await objsmsService.SendSMS(customer.Countrycode.Replace("+", "") + user.PhoneNumber, body);

                    SmsLog objsmslog = new SmsLog()
                    {
                        Sendto = user.PhoneNumber,
                        Body = body,
                        Response = result
                    };

                    InsuranceContext.SmsLogs.Insert(objsmslog);
                }

                var data = (Item)Session["itemData"];
                if (data != null)
                {


                    string userRegisterationEmailPath = "/Views/Shared/EmaiTemplates/UserPaymentEmail.cshtml";
                    string EmailBody2 = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(userRegisterationEmailPath));
                    var Body2 = EmailBody2.Replace("#DATE#", DateTime.Now.ToShortDateString()).Replace("#FirstName#", customer.FirstName).Replace("#LastName#", customer.LastName).Replace("#AccountName#", customer.FirstName + ", " + customer.LastName).Replace("#Address1#", customer.AddressLine1).Replace("#Address2#", customer.AddressLine2).Replace("#Amount#",Convert.ToString(summaryDetail.TotalPremium)).Replace("#PaymentDetails#", "New Premium").Replace("#ReceiptNumber#", policy.PolicyNumber).Replace("#PaymentType#", (summaryDetail.PaymentMethodId == 1 ? "Bank" : (summaryDetail.PaymentMethodId == 2 ? "Cash" : "Visa")));
                    objEmailService.SendEmail(user.Email, "", "", "Payment", Body2, null);
                }
                
                Insurance.Service.VehicleService obj = new Insurance.Service.VehicleService();
                VehicleModel model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{vehicle.ModelId}'");
                VehicleMake make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{vehicle.MakeId}'");
                
                string vehicledescription = model.ModelDescription + " / " + make.MakeDescription;

                decimal totalpaymentdue = 0.00m;

                if (summaryDetail.PaymentTermId == 1)
                {
                    totalpaymentdue = (decimal)summaryDetail.TotalPremium;
                }
                else if (summaryDetail.PaymentTermId == 4)
                {
                    totalpaymentdue = (decimal)summaryDetail.TotalPremium * 3;
                }
                else if (summaryDetail.PaymentTermId == 3)
                {
                    totalpaymentdue = (decimal)summaryDetail.TotalPremium * 4;
                }


                string Summeryofcover = "";
                for (int i = 0; i < vehicle.NoOfCarsCovered; i++)
                {
                    Summeryofcover += "<tr><td>" + vehicledescription + "</td><td>$" + vehicle.SumInsured + "</td><td>" + (vehicle.CoverTypeId == 1 ? eCoverType.Comprehensive.ToString() : eCoverType.ThirdParty.ToString()) + "</td><td>" + InsuranceContext.VehicleUsages.All(vehicle.VehicleUsage).Select(x => x.VehUsage).FirstOrDefault() + "</td><td>$0.00</td><td>$" + Convert.ToString(vehicle.Excess) + "</td><td>$" + Convert.ToString(summaryDetail.TotalPremium) + "</td></tr>";
                }

                //TempData["Summeryofcover"] = Summeryofcover;

                var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm)) select new { ID = (int)e, Name = e.ToString() };
                var paymentTerm = ePaymentTermData.FirstOrDefault(p => p.ID == summaryDetail.PaymentTermId);
                string SeheduleMotorPath = "/Views/Shared/EmaiTemplates/SeheduleMotor.cshtml";
                string MotorBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(SeheduleMotorPath));
                var Bodyy = MotorBody.Replace("##PolicyNo##", policy.PolicyNumber).Replace("##Cellnumber##", user.PhoneNumber).Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##Email##", user.Email).Replace("##BirthDate##", customer.DateOfBirth.Value.ToString("dd/MM/yyyy")).Replace("##Address1##", customer.AddressLine1).Replace("##Address2##", customer.AddressLine2).Replace("##Renewal##", policy.RenewalDate.Value.ToString("dd/MM/yyyy")).Replace("##InceptionDate##", policy.StartDate.Value.ToString("dd/MM/yyyy")).Replace("##package##", paymentTerm.Name).Replace("##Summeryofcover##", Summeryofcover).Replace("##PaymentTerm##", (summaryDetail.PaymentTermId == 1 ? paymentTerm.Name + "(1 Year)" : paymentTerm.Name + "(" + summaryDetail.PaymentTermId.ToString() + "Months)")).Replace("##TotalPremiumDue##", Convert.ToString(totalpaymentdue)).Replace("##StampDuty##", Convert.ToString(summaryDetail.TotalStampDuty)).Replace("##MotorLevy##",Convert.ToString(summaryDetail.TotalZTSCLevies)).Replace("##PremiumDue##",Convert.ToString(summaryDetail.TotalPremium)).Replace("##PostalAddress##",customer.Zipcode);
                objEmailService.SendEmail(user.Email, "", "", "Sehedule-motor", Bodyy, null);
            }



            Session.Remove("PolicyData");
            Session.Remove("VehicleDetail");
            Session.Remove("policytermid");
            Session.Remove("SummaryDetailed");
            Session.Remove("CardDetail");

            return View(objSaveDetailListModel);
        }
        [HttpPost]
        public ActionResult SaveDetailList(PaymentInformationsModel model)
        {

            return View();
        }

        [HttpPost]
        public ActionResult SaveDeleverLicence(bool IsCheck, int Id)
        {
            try
            {
                var paymentInformations = InsuranceContext.PaymentInformations.SingleCustome(Id);
                paymentInformations.DeleverLicence = IsCheck;
                InsuranceContext.PaymentInformations.Update(paymentInformations);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }

        }
        public async Task<ActionResult> InitiatePaynowTransaction(Int32 id, string TotalPremium, string PolicyNumber, string Email)
        {
            Insurance.Service.PaynowService paynowservice = new Insurance.Service.PaynowService();
            PaynowResponse paynowresponse = new PaynowResponse();

            paynowresponse = await paynowservice.initiateTransaction("reference", TotalPremium, PolicyNumber, Email);

            if (paynowresponse.status == "Ok")
            {
                string strScript = "window.open('" + paynowresponse.browserurl + "', 'Confirm Payment','width = 800, height = 800','_blank');";
                ViewBag.strScript = "<script type='text/javascript'>$(document).ready(function(){" + strScript + "});</script>";
            }
            else
            {
                ViewBag.strScript = "<script type='text/javascript'>$(document).ready(function(){$('#errormsg').text('" + paynowresponse.error + "');});</script>";
            }

            return View();
            //return RedirectToAction("SaveDetailList", "Paypal", new { id = id });
        }
    }
}


