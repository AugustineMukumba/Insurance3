using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class ClsVehicleModel
    {
        public int Id { get; set; }
        public string ModelDescription { get; set; }
        public string ModelCode { get; set; }
        public string ShortDescription { get; set; }
        public string MakeCode { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
    }
}
