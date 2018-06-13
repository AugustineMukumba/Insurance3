using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
     public class VehicleUsageModel
    {
        public int Id { get; set; }
        
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Please enter Veh Usage.")]
        public string VehUsage { get; set; }
        [Required(ErrorMessage = "Please enter Comprehensive Rate.")]
        public Single? ComprehensiveRate { get; set; }
        [Required(ErrorMessage = "Please enter MinComp Amount.")]
        public decimal? MinCompAmount { get; set; }
        [Required(ErrorMessage = "Please enter ThirdParty Rate.")]
        public Single? ThirdPartyRate { get; set; }
        [Required(ErrorMessage = "Please enter MinThird Amount.")]
        public decimal? MinThirdAmount { get; set; }
        [Required(ErrorMessage = "Please enter FTP Amount.")]
        public decimal? FTPAmount { get; set; }
        [Required(ErrorMessage = "Please enter AnnualTP Amount.")]
        public decimal? AnnualTPAmount { get; set; }
    }
}
