using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class SmsLogModel
    {
        public int Id { get; set; }
        public string Sendto { get; set; }
        public string Body { get; set; }
        public string Response { get; set; }

    }
}
