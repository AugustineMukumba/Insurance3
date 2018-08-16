using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using System.Configuration;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using InsuranceClaim.Models;
using Newtonsoft.Json;
using System.Web;

namespace Insurance.Service
{
    public class ICEcashService
    {
        public static string PSK = "127782435202916376850511";
        private static string GetSHA512(string text)
        {
            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] hashValue;
            byte[] message = UE.GetBytes(text);
            SHA512Managed hashString = new SHA512Managed();
            string encodedData = Convert.ToBase64String(message);
            string hex = "";
            hashValue = hashString.ComputeHash(UE.GetBytes(encodedData));
            foreach (byte x in hashValue)
            {
                hex += String.Format("{0:x2}", x);
            }
            return hex;
        }

        public static string SHA512(string input)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            using (var hash = System.Security.Cryptography.SHA512.Create())
            {
                var hashedInputBytes = hash.ComputeHash(bytes);

                // Convert to text
                // StringBuilder Capacity is 128, because 512 bits / 8 bits in byte * 2 symbols for byte 
                var hashedInputStringBuilder = new System.Text.StringBuilder(128);
                foreach (var b in hashedInputBytes)
                    hashedInputStringBuilder.Append(b.ToString("X2"));
                return hashedInputStringBuilder.ToString();
            }
        }

        public ICEcashTokenResponse getToken()
        {

            //string json = "%7B%20%20%20%22PartnerReference%22%3A%20%228eca64cb-ccf8-4304-a43f-a6eaef441918%22%2C%0A%20%20%20%20%22Date%22%3A%20%22201801080615165001%22%2C%0A%20%20%20%20%22Version%22%3A%20%222.0%22%2C%0A%20%20%20%20%22Request%22%3A%20%7B%0A%20%20%20%20%20%20%20%20%22Function%22%3A%20%22PartnerToken%22%7D%7D";
            //string PSK = "127782435202916376850511";
            string _json = "";//"{'PartnerReference':'" + Convert.ToString(Guid.NewGuid()) + "','Date':'" + DateTime.Now.ToString("yyyyMMddhhmmss") + "','Version':'2.0','Request':{'Function':'PartnerToken'}}";
            Arguments objArg = new Arguments();
            objArg.PartnerReference = Guid.NewGuid().ToString();
            objArg.Date = DateTime.Now.ToString("yyyyMMddhhmmss");
            objArg.Version = "2.0";
            objArg.Request = new FunctionObject { Function = "PartnerToken" };

            _json = Newtonsoft.Json.JsonConvert.SerializeObject(objArg);

            //string  = json.Reverse()
            string reversejsonString = new string(_json.Reverse().ToArray());
            string reversepartneridString = new string(PSK.Reverse().ToArray());

            string concatinatedString = reversejsonString + reversepartneridString;

            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(concatinatedString);

            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);

            string GetSHA512encrypted = SHA512(returnValue);

            string MAC = "";

            for (int i = 0; i < 16; i++)
            {
                MAC += GetSHA512encrypted.Substring((i * 8), 1);
            }

            MAC = MAC.ToUpper();


            ICERootObject objroot = new ICERootObject();
            objroot.Arguments = objArg;
            objroot.MAC = MAC;
            objroot.Mode = "SH";

            var data = Newtonsoft.Json.JsonConvert.SerializeObject(objroot);


            JObject jsonobject = JObject.Parse(data);

            var client = new RestClient("http://api-test.icecash.com/request/20523588");
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", jsonobject, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            ICEcashTokenResponse json = JsonConvert.DeserializeObject<ICEcashTokenResponse>(response.Content);

            HttpContext.Current.Session["ICEcashToken"] = json;

            return json;
        }

        public ICEcashQuoteResponse RequestQuote(List<RiskDetailModel> listofvehicles,string PartnerToken)
        {
            //string PSK = "127782435202916376850511";
            string _json = "";

            List<VehicleObject> obj = new List<VehicleObject>();


            foreach (var item in listofvehicles)
            {
                obj.Add(new VehicleObject { VRN = item.RegistrationNo, DurationMonths = (item.PaymentTermId == 1 ? 12 : item.PaymentTermId), VehicleValue = 10000, YearManufacture = Convert.ToInt32(item.VehicleYear), InsuranceType = (item.CoverTypeId == 1 ? 4 : Convert.ToInt32(item.CoverTypeId)), VehicleType = 1, TaxClass = 1, Make = "ACTM", Model = "MONTELIMAR", EntityType = "Personal", Town = "Dummy Town", Address1 = "Add1", Address2 = "Add2", CompanyName = "Dummy Company", FirstName = "Constantine", LastName = "Mambariza", IDNumber = "14-104864Y27", MSISDN = "+2630775308520" });
            }

            QuoteArguments objArg = new QuoteArguments();
            objArg.PartnerReference = Guid.NewGuid().ToString();
            objArg.Date = DateTime.Now.ToString("yyyyMMddhhmmss");
            objArg.Version = "2.0";
            objArg.PartnerToken = PartnerToken;
            objArg.Request = new QuoteFunctionObject { Function = "TPIQuote", Vehicles = obj };

            _json = Newtonsoft.Json.JsonConvert.SerializeObject(objArg);

            //string  = json.Reverse()
            string reversejsonString = new string(_json.Reverse().ToArray());
            string reversepartneridString = new string(PSK.Reverse().ToArray());

            string concatinatedString = reversejsonString + reversepartneridString;

            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(concatinatedString);

            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);

            string GetSHA512encrypted = SHA512(returnValue);

            string MAC = "";

            for (int i = 0; i < 16; i++)
            {
                MAC += GetSHA512encrypted.Substring((i * 8), 1);
            }

            MAC = MAC.ToUpper();


            ICEQuoteRequest objroot = new ICEQuoteRequest();
            objroot.Arguments = objArg;
            objroot.MAC = MAC;
            objroot.Mode = "SH";

            var data = Newtonsoft.Json.JsonConvert.SerializeObject(objroot);


            JObject jsonobject = JObject.Parse(data);

            var client = new RestClient("http://api-test.icecash.com/request/20523588");
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", jsonobject, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            ICEcashQuoteResponse json = JsonConvert.DeserializeObject<ICEcashQuoteResponse>(response.Content);

            return json;
        }
    }

    public class Arguments
    {
        public string PartnerReference { get; set; }
        public string Date { get; set; }
        public string Version { get; set; }
        public FunctionObject Request { get; set; }
    }

    public class FunctionObject
    {
        public string Function { get; set; }
    }

    public class ICERootObject
    {
        public Arguments Arguments { get; set; }
        public string MAC { get; set; }
        public string Mode { get; set; }
    }

    public class VehicleObject
    {
        public string VRN { get; set; }
        public string IDNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MSISDN { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Town { get; set; }
        public string EntityType { get; set; }
        public string CompanyName { get; set; }
        public int DurationMonths { get; set; }
        public int VehicleValue { get; set; }
        public int InsuranceType { get; set; }
        public int VehicleType { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int TaxClass { get; set; }
        public int YearManufacture { get; set; }
    }

    public class QuoteArguments
    {
        public string PartnerReference { get; set; }
        public string Date { get; set; }
        public string Version { get; set; }
        public string PartnerToken { get; set; }
        public QuoteFunctionObject Request { get; set; }
    }

    public class QuoteFunctionObject
    {
        public string Function { get; set; }
        public List<VehicleObject> Vehicles { get; set; }
    }

    public class ICEQuoteRequest
    {
        public QuoteArguments Arguments { get; set; }
        public string MAC { get; set; }
        public string Mode { get; set; }
    }

    public class TokenReposone
    {
        public string Function { get; set; }
        public string Result { get; set; }
        public string Message { get; set; }
        public string PartnerToken { get; set; }
        public string ExpireDate { get; set; }
    }

    public class ICEcashTokenResponse
    {
        public string PartnerReference { get; set; }
        public string Date { get; set; }
        public string Version { get; set; }
        public TokenReposone Response { get; set; }
    }



    public class Quote
    {
        public string VRN { get; set; }
        public string InsuranceID { get; set; }
        public int Result { get; set; }
        public string Message { get; set; }
    }

    public class QuoteResponse
    {
        public int Result { get; set; }
        public string Message { get; set; }
        public List<Quote> Quotes { get; set; }
    }

    public class ICEcashQuoteResponse
    {
        public string PartnerReference { get; set; }
        public string Date { get; set; }
        public string Version { get; set; }
        public QuoteResponse Response { get; set; }
    }
}
