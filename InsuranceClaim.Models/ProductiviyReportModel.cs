using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
  public  class ProductiviyReportModel
    {
        public string UserName { get; set; }
        public string TransactionDate { get; set; }
        public string PolicyNumber { get; set; }
        public string CustomerName{ get; set; }
        //public string LastName { get; set; }
        public string Product { get; set; }
        public decimal SumInsured { get; set; }
        public decimal PremiumDue { get; set; }
    }
    public class ListProductiviyReportModel
    {
        public List<ProductiviyReportModel> ListProductiviyReport { get; set; }
    }
    public class ProductiviySearchReportModel
    {
        public List<ProductiviyReportModel> ListProductiviyReport { get; set; }
    }


}
