using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class ProductModel
    {
        
        public int Id { get; set; }
        [Required(ErrorMessage = "Please enter Product Name.")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Please enter Product Code.")]
        public string ProductCode { get; set; }
        
        public bool? Active { get; set; }
    }
}
