using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class CardDetailModel
    {
        [Required]
        public string CardNumber { get; set; }
        [Required]
        public string NameOnCard { get; set; }
        [Required]
        public string ExpiryDate { get; set; }
        [Required]
        public string CVC { get; set; }
    }
}
