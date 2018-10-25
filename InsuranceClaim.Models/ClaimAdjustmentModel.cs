using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
   public  class ClaimAdjustmentModel
    {
        public int Id { get; set; }
        [Display(Name = "Amount To Pay")]
        [Required(ErrorMessage = "Please Enter Amount To Pay.")]
        public int AmountToPay { get; set; }
        [Display(Name = "Estimated Loss Amount")]
        [Required(ErrorMessage = "Please Enter Estimated Loss Amount.")]
        public int EstimatedLoss { get; set; }
        [Display(Name = "Excesses Amount")]
        [Required(ErrorMessage = "Please Enter Excesses Amount.")]
        public string ExcessesAmount { get; set; }
        [Display(Name = "Payee Bank Details")]
        [Required(ErrorMessage = "Please Enter Payee Bank Details.")]
        public string PayeeBankDetails { get; set; }
        [Display(Name = "First Name")]
        [Required(ErrorMessage = "Please Enter First Name.")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "Please Enter Last Name.")]
        public string LastName { get; set; }
        [Display(Name = "Payee/Claimant Name ")]
        [Required(ErrorMessage = "Please Enter Last Name.")]
        public string PayeeName { get; set; }
        [Display(Name = "Policy Holder Name")]
        [Required(ErrorMessage = "Please Enter Policy Holder Name.")]
        public string PolicyholderName { get; set; }
        [Display(Name = "Driver Is Under 21")]
        [Required(ErrorMessage = "This is required field")]
        public bool DriverIsUnder21 { get; set; }

        [Display(Name = "Is licensed less 60 months")]
        [Required(ErrorMessage = "This is required field")]
        public bool Islicensedless60months { get; set; }
        [Display(Name = "Is Stolen")]
        [Required(ErrorMessage = "This is required field")]
        public bool IsStolen { get; set; }
        [Display(Name = "Is Loss In Zimbabwe")]
        [Required(ErrorMessage = "This is required field")]
        public bool IsLossInZimbabwe { get; set; }
        [Display(Name = "Is Partial Loss")]
        [Required(ErrorMessage = "This is required field")]
        public bool IsPartialLoss { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int ModifiedBy { get; set; }
        public bool IsActive { get; set; }
    }
}
