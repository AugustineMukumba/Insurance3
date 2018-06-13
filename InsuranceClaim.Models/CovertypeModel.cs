using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class CovertypeModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Please enter Name.")]
        public string Name { get; set; }
    }
}
