using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace InsuranceClaim.Models
{
  public  class ClaimNotificationModel
    {
        public int Id { get; set; }  
        [Required(ErrorMessage = "Please Enter Policy Number")]
        [Display(Name = "Policy Number")]
        public string PolicyNumber { get; set; }

        [Required(ErrorMessage = "Please Enter Date")]
        [Display(Name = "Date Of Loss")]
        public DateTime DateOfLoss { get; set; }

        [Required(ErrorMessage = "Please Enter Place")]
        [Display(Name = "Place Of Loss")]
        public string PlaceOfLoss { get; set; }

        [Required(ErrorMessage = "Please Enter Description")]
        [Display(Name = "Description Of Loss")]
        public string DescriptionOfLoss { get; set; }

        [Required(ErrorMessage = "Please Enter Estimated Value")]
        [Display(Name = "Estimated Value Of Loss")]
        public decimal EstimatedValueOfLoss { get; set; }

        [Required(ErrorMessage = "Please Enter Third Party Involvement")]
        [Display(Name = "Third Party Involvement")]
        public string ThirdPartyInvolvement { get; set; }
        //public int CreatedBy { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsRegistered { get; set; }
        
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public bool IsExists { get; set; }

    }
}
