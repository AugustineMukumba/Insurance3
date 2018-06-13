using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
  public class PolicyInsurerModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Please enter Insurer Name.")]
        public string InsurerName { get; set; }
        [Required(ErrorMessage = "Please enter Insurer Code.")]
        public string InsurerCode { get; set; }
        [Required(ErrorMessage = "Please enter Insurer Address.")]
        public string InsurerAddress { get; set; }
       
    }
}
