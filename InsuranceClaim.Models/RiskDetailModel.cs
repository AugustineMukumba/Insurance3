using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class RiskDetailModel
    {
        public int Id { get; set; }
        public int PolicyId { get; set; }
        [Required(ErrorMessage = "Please Enter No Of Cars Covered")]
        public int? NoOfCarsCovered { get; set; }
        [Required(ErrorMessage = "Please Enter Registration No")]
        public string RegistrationNo { get; set; }
        public int? CustomerId { get; set; }
        public string MakeId { get; set; }
        public string ModelId { get; set; }
        [Required(ErrorMessage = "Please Enter Cubic Capacity")]
        public decimal? CubicCapacity { get; set; }
        [Required(ErrorMessage = "Please Enter Vehicle Year")]
        public int? VehicleYear { get; set; }
        [Required(ErrorMessage = "Please Enter Engine Number")]
        public string EngineNumber { get; set; }
        [Required(ErrorMessage = "Please Enter Chassis Number")]
        public string ChasisNumber { get; set; }
        public string VehicleColor { get; set; }
        public string VehicleUsage { get; set; }
        public int? CoverTypeId { get; set; }
        [Required(ErrorMessage = "Please Enter Cover Start Date")]
        public DateTime? CoverStartDate { get; set; }
        [Required(ErrorMessage = "Please Enter Cover End Date")]

        public DateTime? CoverEndDate { get; set; }
        [Required(ErrorMessage = "Please Enter Sum Insured")]

        public decimal? SumInsured { get; set; }
        public decimal? Premium { get; set; }
        public int? AgentCommissionId { get; set; }
        public decimal? Rate { get; set; }
        public decimal? StampDuty { get; set; }
        public decimal? ZTSCLevy { get; set; }
        public decimal? RadioLicenseCost { get; set; }
        public string OptionalCovers { get; set; }
        public int ExcessType { get; set; }
        public decimal Excess { get; set; }
        public string CoverNoteNo { get; set; }
    }
}
