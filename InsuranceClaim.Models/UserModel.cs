using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class UserModel
    {

        [Required(ErrorMessage = "Please Enter Email.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please Enter Current Password.")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Please Enter New Password.")]
        public string NewPassword { get; set; }
        [System.Web.Mvc.Compare("NewPassword", ErrorMessage = "The NewPassword and ConfirmPassword Password do not Match.")]
        public string ConfirmPassword { get; set; }


    }
}
