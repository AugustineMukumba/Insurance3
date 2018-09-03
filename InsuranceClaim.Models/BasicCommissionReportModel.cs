using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class BasicCommissionReportModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PolicyNumber { get; set; }
        public string TransactionDate { get; set; }
        public decimal? SumInsured { get; set; }
        public decimal? Premium { get; set; }
        public decimal? Commission { get; set; }
        public decimal? ManagementCommission { get; set; }
    }


    public class ListBasicCommissionReport
    {
        public List<BasicCommissionReportModel> BasicCommissionReport { get; set; }
    }
}
