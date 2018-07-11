using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace InsuranceClaim.Models
{
    public class PolicyDetailModel
    {
        public int Id { get; set; }
        //[Display(Name = "Policy Name")]
        [Required(ErrorMessage = "Please enter policy name.")]
        [MaxLength(50,ErrorMessage ="Policy name must be less than 50 characters long.")]
        public string PolicyName { get; set; }
        //[Display(Name ="Policy Number")]
        [Required(ErrorMessage ="Please enter policy number.")]
        [MaxLength(25,ErrorMessage ="Policy number must be less than 25 characters long.")]
        public string PolicyNumber { get; set; }
        [Display(Name = "Insurer")]
        public int? InsurerId { get; set; }
        //[Display(Name = "Payment Term")]
        [Required(ErrorMessage = "Please Select policy Term.")]
        public int? PaymentTermId { get; set; }
        //[Display(Name = "Policy Status")]
        public int PolicyStatusId { get; set; }
        //[Display(Name = "Currency")]
        public int CurrencyId { get; set; }
        //[Display(Name ="Start Date")]
        [Required(ErrorMessage ="Please enter policy start date")]        
        public DateTime? StartDate { get; set; }
        //[Display(Name = "End Date")]
        [Required(ErrorMessage = "Please enter policy End date.")]
        public DateTime? EndDate { get; set; }
        //[Display(Name = "Renewal Date")]
        public DateTime? RenewalDate { get; set; }
        [Display(Name = "Transaction Date")]
        public DateTime? TransactionDate { get; set; }
        [Display(Name = "Business Source")]
        public int BusinessSourceId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
        public int PolicyTerm { get; set; }
    }
}
