using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class RiskDetailModel
    {
        public int Id { get; set; }
        public int? NoOfCarsCovered { get; set; }
        public string RegistrationNo { get; set; }
        public int? CustomerId { get; set; }
        public string  MakeId { get; set; }
        public string  ModelId { get; set; }
        public decimal? CubicCapacity { get; set; }
        public int? VehicleYear { get; set; }
        public string EngineNumber { get; set; }
        public string ChasisNumber { get; set; }
        public string VehicleColor { get; set; }
        public string VehicleUsage { get; set; }
        public int? CoverTypeId { get; set; }
        public DateTime? CoverStartDate { get; set; }
        public DateTime? CoverEndDate { get; set; }
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
