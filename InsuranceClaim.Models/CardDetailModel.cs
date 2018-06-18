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
        [Required(ErrorMessage = "Please Enter Card Number")]
        public string CardNumber { get; set; }
        [Required(ErrorMessage = "Please Enter Name of Card")]
        public string NameOnCard { get; set; }
        [Required(ErrorMessage = "Please Enter Expiry Date ")]
        public string ExpiryDate { get; set; }
        [Required(ErrorMessage ="Please Enter CVC")]
        public string CVC { get; set; }

        public int SummaryDetailId { get; set; }
    }
}
