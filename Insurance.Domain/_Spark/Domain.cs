
using System;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.Generic;

namespace Insurance.Domain
{
    // ############################################################################
    // #
    // #        ---==>  T H I S  F I L E  W A S  G E N E R A T E D  <==---
    // #
    // # This file was generated by PRO Spark, C# Edition, Version 4.5
    // # Generated on 2/26/2017 7:39:39 PM
    // #
    // # Edits to this file may cause incorrect behavior and will be lost
    // # if the code is regenerated.
    // #
    // ############################################################################

    // Insurance Domain objects

    public partial class Currency : Entity<Currency>
    {
        public Currency() { }
        public Currency(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
    }

    public partial class AgentCommission : Entity<AgentCommission>
    {
        public AgentCommission() { }
        public AgentCommission(bool defaults) : base(defaults) { }

        public int AgentCommissionId { get; set; }
        public string CommissionName { get; set; }
        public float? CommissionAmount { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
    }

    public partial class BusinessSource : Entity<BusinessSource>
    {
        public BusinessSource() { }
        public BusinessSource(bool defaults) : base(defaults) { }

        public int BusinessSourceId { get; set; }
        public string Source { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
    }

    public partial class CoverType : Entity<CoverType>
    {
        public CoverType() { }
        public CoverType(bool defaults) : base(defaults) { }

        public int CoverTypeId { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
    }

    public partial class Customer : Entity<Customer>
    {
        public Customer() { }
        public Customer(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public int UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zipcode { get; set; }
        public string Country { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public bool? IsWelcomeNoteSent { get; set; }
        public bool? IsPolicyDocSent { get; set; }
        public bool? IsLicenseDiskNeeded { get; set; }
        public bool? IsOTPConfirmed { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
    }

    public partial class LicenseDelivery : Entity<LicenseDelivery>
    {
        public LicenseDelivery() { }
        public LicenseDelivery(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public string TicketNo { get; set; }
        public int? CustomerId { get; set; }
        public int? UserId { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public bool? IsAcive { get; set; }
    }

    public partial class LoyaltyDetail : Entity<LoyaltyDetail>
    {
        public LoyaltyDetail() { }
        public LoyaltyDetail(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public int? PolicyId { get; set; }
        public int? PointsEarned { get; set; }
        public DateTime? EarnedDate { get; set; }
        public int? PointsRedemed { get; set; }
        public DateTime? RedemedDate { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
    }

    public partial class PaymentMethod : Entity<PaymentMethod>
    {
        public PaymentMethod() { }
        public PaymentMethod(bool defaults) : base(defaults) { }

        public int PaymentMethodId { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
    }

    public partial class PaymentTerm : Entity<PaymentTerm>
    {
        public PaymentTerm() { }
        public PaymentTerm(bool defaults) : base(defaults) { }

        public int PaymentTermId { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
    }

    public partial class PolicyDetail : Entity<PolicyDetail>
    {
        public PolicyDetail() { }
        public PolicyDetail(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public string PolicyName { get; set; }
        public string PolicyNumber { get; set; }
        public int? InsurerId { get; set; }
        public int PolicyStatusId { get; set; }
        public int CurrencyId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? RenewalDate { get; set; }
        public DateTime? TransactionDate { get; set; }
        public int BusinessSourceId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
    }

    public partial class PolicyStatus : Entity<PolicyStatus>
    {
        public PolicyStatus() { }
        public PolicyStatus(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
    }

    public partial class SummaryDetail : Entity<SummaryDetail>
    {
        public SummaryDetail() { }
        public SummaryDetail(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public int? PaymentTermId { get; set; }
        public int? PaymentMethodId { get; set; }
        public decimal? TotalSumInsured { get; set; }
        public decimal? TotalPremium { get; set; }
        public decimal? TotalStampDuty { get; set; }
        public decimal? TotalZTSCLevies { get; set; }
        public decimal? TotalRadioLicenseCost { get; set; }
        public string DebitNote { get; set; }
        public string ReceiptNumber { get; set; }
        public bool? SMSConfirmation { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
    }

    public partial class VehicleDetail : Entity<VehicleDetail>
    {
        public VehicleDetail() { }
        public VehicleDetail(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public int? NoOfCarsCovered { get; set; }
        public string RegistrationNo { get; set; }
        public int? CustomerId { get; set; }
        public int? MakeId { get; set; }
        public int? ModelId { get; set; }
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
        public string Excess { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
    }

    public partial class VehicleMake : Entity<VehicleMake>
    {
        public VehicleMake() { }
        public VehicleMake(bool defaults) : base(defaults) { }

        public int? MakeId { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
    }

    public partial class VehicleModel : Entity<VehicleModel>
    {
        public VehicleModel() { }
        public VehicleModel(bool defaults) : base(defaults) { }

        public int ModelId { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
    }

    public partial class BasicExcess : Entity<BasicExcess>
    {
        public BasicExcess() { }
        public BasicExcess(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public string Type { get; set; }
        public decimal? TotalLoss { get; set; }
        public decimal? PartialLoss { get; set; }
        public decimal? OutSideOfZimbabwe { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
    }

    public partial class AgeExcess : Entity<AgeExcess>
    {
        public AgeExcess() { }
        public AgeExcess(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public string ExcessType { get; set; }
        public int? Age { get; set; }
        public decimal? AgeExcessAmount { get; set; }
        public bool? LicencedLessThan60Months { get; set; }
        public decimal? LicencedLessThan60ExcessAmount { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
    }

    public partial class SpecificExcess : Entity<SpecificExcess>
    {
        public SpecificExcess() { }
        public SpecificExcess(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public string ExcessType { get; set; }
        public string VehicleParts { get; set; }
        public decimal? ExcessAmount { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? Createdby { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
    }
}