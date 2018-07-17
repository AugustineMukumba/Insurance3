using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Insurance.Service
{

    public class PaynowService
    {

        private static readonly HttpClient client = new HttpClient();
        private static String IntegrationID = "5623";
        private static string IntegrationKey = "7c1cd190-5046-4292-806a-0dbb85b949f6";

        public async Task<InsuranceClaim.Models.PaynowResponse> initiateTransaction(string reference, string amount, string additionalinfo, string authemail)
        {
            InsuranceClaim.Models.PaynowResponse paynowresponse = new InsuranceClaim.Models.PaynowResponse();

            string PaymentId = "PAYNOW-" + Guid.NewGuid().ToString();
            HttpContext.Current.Session["PaymentId"] = PaymentId;

            var values = new Dictionary<string, string>
            {

               { "resulturl", "http://localhost:49872/PayNow/Index"},
               { "returnurl", "http://localhost:49872/PayNow/Index" },
               { "reference", PaymentId },
               { "amount", "5.00" },
               { "id", IntegrationID },
               { "additionalinfo", "additional" },
               { "authemail", "ankit.dhiman@kindlebit.com" },
               { "status", "Message" }
            };

            var generatedhash = GenerateTwoWayHash(values, new Guid(IntegrationKey));            

            var _values = new Dictionary<string, string>
            {
               { "resulturl", "http://localhost:49872/PayNow/Index"},
               { "returnurl", "http://localhost:49872/PayNow/Index" },
               { "reference", PaymentId },
               { "amount", "5.00" },
               { "id", IntegrationID },
               { "additionalinfo", "additional" },
               { "authemail", "ankit.dhiman@kindlebit.com" },
               { "status", "Message" },
               { "hash", generatedhash.ToUpper() }
            };

            paynowresponse.generatedhash = generatedhash.ToUpper();

            var content = new FormUrlEncodedContent(_values);

            var response = await client.PostAsync("https://www.paynow.co.zw/interface/initiatetransaction", content);

            var responseString = await response.Content.ReadAsStringAsync();

            string decodedUrl = Uri.UnescapeDataString("http://dummyurl_to_decode_asURL.com?" + responseString);

            Uri responseUri = new Uri(decodedUrl);



            if (decodedUrl.Contains("Status=Error"))
            {
                paynowresponse.status = HttpUtility.ParseQueryString(responseUri.Query).Get("status");
                paynowresponse.error = HttpUtility.ParseQueryString(responseUri.Query).Get("error");

                //log error
                //display error
                return paynowresponse;
            }
            else
            {
                paynowresponse.browserurl = HttpUtility.ParseQueryString(responseUri.Query).Get("browserurl");
                paynowresponse.pollurl = HttpUtility.ParseQueryString(responseUri.Query).Get("pollurl");
                paynowresponse.status = HttpUtility.ParseQueryString(responseUri.Query).Get("status");
                paynowresponse.hash = HttpUtility.ParseQueryString(responseUri.Query).Get("hash");

                //log success
                //display success
                //It is vital that the merchant site verify the hash value contained in the message before redirecting the
                //Customer to the browserurl. 
                return paynowresponse;
            }

        }

        public string GenerateTwoWayHash(Dictionary<string, string> items, Guid guid)
        {
            string concat = string.Join("", items.Select(c => (c.Value != null ? c.Value.Trim() : "")).ToArray());
            SHA512 check = SHA512.Create();
            byte[] resultArr = check.ComputeHash(Encoding.UTF8.GetBytes(concat + guid));
            return ByteArrayToString(resultArr);
        }

        public string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

    }
}
