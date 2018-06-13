using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class AgentCommissionModel
    {
        public int id { get; set; }
        public string CommissionName { get; set; }
        public Single CommissionAmount { get; set; }
        public Single ManagementCommission { get; set; }
    }
}
