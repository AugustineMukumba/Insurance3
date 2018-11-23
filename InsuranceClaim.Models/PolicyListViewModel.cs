using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class PolicyListViewModel
    {

        public List<VehicleReinsuranceViewModel> Vehicles { get; set; }
        public string PolicyNumber { get; set; }
        public int CustomerId { get; set; }
        public int PaymentMethodId { get; set; }
        public decimal TotalSumInsured { get; set; }
        public decimal TotalPremium { get; set; }
        public int SummaryId { get; set; }

    }

    public class ListPolicy
    {
        public List<PolicyListViewModel> listpolicy { get; set; }
    }

    public class VehicleReinsuranceViewModel
    {
        public int VehicleId { get; set; }
        public string MakeId { get; set; }
        public string ModelId { get; set; }
        public bool isReinsurance { get; set; }
        public decimal ReinsuranceAmount { get; set; }
        public int ReinsurerBrokerId { get; set; }
        public decimal SumInsured { get; set; }
        public decimal Premium { get; set; }
        public string RegisterationNumber { get; set; }
        public int CoverType { get; set; }
        public DateTime startdate { get; set; }
        public DateTime enddate { get; set; }


    }
}
