﻿using System;
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
        [Required(ErrorMessage = "Please Enter Commission Name.")]
        public string CommissionName { get; set; }
        [Required(ErrorMessage = "Please Enter Commission Amount.")]
        public double? CommissionAmount { get; set; }
        [Required(ErrorMessage = "Please Enter Management Commission.")]
        public double? ManagementCommission { get; set; }
        public bool? IsActive { get; set; }
    }
}
