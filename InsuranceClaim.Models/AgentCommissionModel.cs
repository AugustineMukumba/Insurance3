using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class AgentCommissionModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Please enter Commission Name.")]
        public string CommissionName { get; set; }
        [Required(ErrorMessage = "Please enter Commission Amount.")]
        public double? CommissionAmount { get; set; }
        [Required(ErrorMessage = "Please enter ManagementCommission.")]
        public double? ManagementCommission { get; set; }
        public Boolean IsActive { get; set; }
    }
}
