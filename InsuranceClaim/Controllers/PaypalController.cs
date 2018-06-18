using InsuranceClaim.Models;
using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Insurance.Domain;
namespace InsuranceClaim.Controllers
{
    public class PaypalController : Controller
    {
        //
        // GET: /Paypal/

        public ActionResult Index(int id)
        {

            return View();
        }

        public ActionResult PaymentWithCreditCard(CardDetailModel model)
        {
            //create and item for which you are taking payment
            //if you need to add more items in the list
            //Then you will need to create multiple item objects or use some loop to instantiate object
            var summaryDetail = InsuranceContext.SummaryDetails.Single(model.SummaryDetailId);
            var vehicle = InsuranceContext.VehicleDetails.Single(summaryDetail.VehicleDetailId);
            var policy = InsuranceContext.PolicyDetails.Single(vehicle.PolicyId);
            var customer = InsuranceContext.Customers.Single(summaryDetail.CustomerId);
            var product = InsuranceContext.Products.Single(Convert.ToInt32(policy.PolicyName));
            var currency = InsuranceContext.Currencies.Single(policy.CurrencyId);

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


            Item item = new Item();
            item.name = product.ProductName;
            item.currency = currency.Name;
            item.price = totalPremium.ToString() + zeros;
            //item.quantity = vehicle.NoOfCarsCovered.ToString();
            item.quantity = "1";
            item.sku = "sku";

            //Now make a List of Item and add the above item to it
            //you can create as many items as you want and add to this list
            List<Item> itms = new List<Item>();
            itms.Add(item);
            ItemList itemList = new ItemList();
            itemList.items = itms;

            //Address for the payment
            Address billingAddress = new Address();
            billingAddress.city = "NewYork";
            billingAddress.country_code = "US";
            billingAddress.line1 = customer.AddressLine1 == string.Empty ? customer.AddressLine2 : customer.AddressLine1;
            billingAddress.postal_code = "43210";
            billingAddress.state = "NY";


            //Now Create an object of credit card and add above details to it
            PayPal.Api.CreditCard crdtCard = new PayPal.Api.CreditCard();
            crdtCard.billing_address = billingAddress;
            crdtCard.cvv2 = model.CVC;
            crdtCard.expire_month = Convert.ToInt32(model.ExpiryDate.Split('/')[0]);
            crdtCard.expire_year = Convert.ToInt32(model.ExpiryDate.Split('/')[1]);
            crdtCard.first_name = model.NameOnCard;
            //crdtCard.last_name = "Thakur";
            crdtCard.number = model.CardNumber;
            crdtCard.type = CreditCardUtility.GetTypeName(model.CardNumber).ToLower();

            // Specify details of your payment amount.
            Details details = new Details();
            details.shipping = "0.00";
            details.subtotal = totalPremium.ToString() + zeros;
            details.tax = "0.00";

            // Specify your total payment amount and assign the details object
            Amount amnt = new Amount();
            amnt.currency = currency.Name;
            // Total = shipping tax + subtotal.
            amnt.total = totalPremium.ToString() + zeros;
           // amnt.details = details;

            // Now make a trasaction object and assign the Amount object
            Transaction tran = new Transaction();
            tran.amount = amnt;
            tran.description = "Online vehicle insurance purchased";
            tran.item_list = itemList;
            tran.invoice_number = summaryDetail.DebitNote;

            // Now, we have to make a list of trasaction and add the trasactions object
            // to this list. You can create one or more object as per your requirements

            List<Transaction> transactions = new List<Transaction>();
            transactions.Add(tran);

            // Now we need to specify the FundingInstrument of the Payer
            // for credit card payments, set the CreditCard which we made above

            FundingInstrument fundInstrument = new FundingInstrument();
            fundInstrument.credit_card = crdtCard;

            // The Payment creation API requires a list of FundingIntrument

            List<FundingInstrument> fundingInstrumentList = new List<FundingInstrument>();
            fundingInstrumentList.Add(fundInstrument);

            // Now create Payer object and assign the fundinginstrument list to the object
            Payer payr = new Payer();
            payr.funding_instruments = fundingInstrumentList;
            payr.payment_method = "credit_card";

            // finally create the payment object and assign the payer object & transaction list to it
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

                //if the createdPayment.State is "approved" it means the payment was successfull else not

                if (createdPayment.state.ToLower() != "approved")
                {
                    ModelState.AddModelError("PaymentError", "Payment not approved");
                    return View("PaymentDetail");
                }
            }
            catch (PayPal.PayPalException ex)
            {
                Logger.Log("Error: " + ex.Message);
                ModelState.AddModelError("PaymentError", ex.Message);
                return View("PaymentDetail");
            }

            return View("SuccessView");
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
    }
}
