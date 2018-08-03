using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
   public class ZTSCLevyReportModels
    {
        public string Customer_Name { get; set; }
        public string Policy_Number { get; set; }
        public  string Transaction_date { get; set; }
        public decimal Premium_due { get; set; }
        public decimal ZTSCLevy { get; set; }
    }

    public class ListZTSCLevyReportModels
    {
        public List<ZTSCLevyReportModels> ListZTSCreportdata { get; set; }
    }
    }
