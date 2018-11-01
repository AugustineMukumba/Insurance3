
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

    }

    public partial class AgentCommission : Entity<AgentCommission>
    {
        public AgentCommission() { }
        public AgentCommission(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public string CommissionName { get; set; }
        public double? CommissionAmount { get; set; }
        public double? ManagementCommission { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }

    }

    public partial class BusinessSource : Entity<BusinessSource>
    {
        public BusinessSource() { }
        public BusinessSource(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public string Source { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }

    }

    public partial class CoverType : Entity<CoverType>
    {
        public CoverType() { }
        public CoverType(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
    }

    public partial class Customer : Entity<Customer>
    {
        public Customer() { }
        public Customer(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public decimal CustomerId { get; set; }
        public string UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string NationalIdentificationNumber { get; set; }
        public string Zipcode { get; set; }
        public string Country { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public bool? IsWelcomeNoteSent { get; set; }
        public bool? IsPolicyDocSent { get; set; }
        public bool? IsLicenseDiskNeeded { get; set; }
        public bool? IsOTPConfirmed { get; set; }
        public string PhoneNumber { get; set; }

        public bool? IsActive { get; set; }
        public string Countrycode { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }

        public bool IsCustomEmail { get; set; }

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

        public bool? IsAcive { get; set; }
    }

    public partial class LoyaltyDetail : Entity<LoyaltyDetail>
    {
        public LoyaltyDetail() { }
        public LoyaltyDetail(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public int? PolicyId { get; set; }
        public decimal? PointsEarned { get; set; }
        public DateTime? EarnedDate { get; set; }
        public int? PointsRedemed { get; set; }
        public DateTime? RedemedDate { get; set; }

        public bool? IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
    }

    public partial class PaymentMethod : Entity<PaymentMethod>
    {
        public PaymentMethod() { }
        public PaymentMethod(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public string Name { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
    }

    public partial class PaymentTerm : Entity<PaymentTerm>
    {
        public PaymentTerm() { }
        public PaymentTerm(bool defaults) : base(defaults) { }

        public int Id { get; set; }
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
        public int CustomerId { get; set; }
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
        public bool? IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
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
        public int? VehicleDetailId { get; set; }
        public int? CustomerId { get; set; }
        public int? PaymentTermId { get; set; }
        public int? PaymentMethodId { get; set; }
        public decimal? TotalSumInsured { get; set; }
        public decimal? TotalPremium { get; set; }
        public decimal? TotalStampDuty { get; set; }
        public decimal? TotalZTSCLevies { get; set; }
        public decimal? TotalRadioLicenseCost { get; set; }
        public decimal? AmountPaid { get; set; }
        public string DebitNote { get; set; }
        public string ReceiptNumber { get; set; }
        public bool? SMSConfirmation { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? BalancePaidDate { get; set; }
        public string Notes { get; set; }
        public bool isQuotation { get; set; }
    }

    public partial class VehicleDetail : Entity<VehicleDetail>
    {
        public VehicleDetail() { }
        public VehicleDetail(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public int PolicyId { get; set; }
        public int? NoOfCarsCovered { get; set; }
        public string RegistrationNo { get; set; }
        public int? CustomerId { get; set; }
        public string MakeId { get; set; }
        public string ModelId { get; set; }
        public decimal? CubicCapacity { get; set; }
        public int? VehicleYear { get; set; }
        public string EngineNumber { get; set; }
        public string ChasisNumber { get; set; }
        public string VehicleColor { get; set; }
        public int? VehicleUsage { get; set; }
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
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
        public Boolean Addthirdparty { get; set; }
        public decimal? AddThirdPartyAmount { get; set; }
        public Boolean PassengerAccidentCover { get; set; }
        public Boolean ExcessBuyBack { get; set; }
        public Boolean RoadsideAssistance { get; set; }
        public Boolean MedicalExpenses { get; set; }
        public int? NumberofPersons { get; set; }
        public bool? IsLicenseDiskNeeded { get; set; }
        public decimal? PassengerAccidentCoverAmount { get; set; }
        public decimal? ExcessBuyBackAmount { get; set; }
        public decimal? RoadsideAssistanceAmount { get; set; }
        public decimal? MedicalExpensesAmount { get; set; }
        public decimal? PassengerAccidentCoverAmountPerPerson { get; set; }
        public decimal? ExcessBuyBackPercentage { get; set; }
        public decimal? RoadsideAssistancePercentage { get; set; }
        public decimal? MedicalExpensesPercentage { get; set; }
        public decimal? ExcessAmount { get; set; }
        public DateTime? RenewalDate { get; set; }
        public DateTime? TransactionDate { get; set; }
        public int PaymentTermId { get; set; }
        public int ProductId { get; set; }
        public bool? IncludeRadioLicenseCost { get; set; }
        public string InsuranceId { get; set; }
        public decimal? AnnualRiskPremium { get; set; }
        public decimal? TermlyRiskPremium { get; set; }
        public decimal? QuaterlyRiskPremium { get; set; }
        public decimal? Discount { get; set; }
        public bool isLapsed { get; set; }
        public decimal? BalanceAmount { get; set; }
        public decimal? VehicleLicenceFee { get; set; }
    }

    public partial class VehicleMake : Entity<VehicleMake>
    {
        public VehicleMake() { }
        public VehicleMake(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public string MakeDescription { get; set; }
        public string MakeCode { get; set; }
        public string ShortDescription { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }

    }

    public partial class VehicleModel : Entity<VehicleModel>
    {
        public VehicleModel() { }
        public VehicleModel(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public string ModelDescription { get; set; }
        public string ModelCode { get; set; }
        public string ShortDescription { get; set; }
        public string MakeCode { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }

    }

    public partial class BasicExcess : Entity<BasicExcess>
    {
        public BasicExcess() { }
        public BasicExcess(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public string ExcessType { get; set; }
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
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
    }
    public partial class ReinsuranceBroker : Entity<ReinsuranceBroker>
    {
        public ReinsuranceBroker() { }
        public ReinsuranceBroker(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public string ReinsuranceBrokerCode { get; set; }
        public string ReinsuranceBrokerName { get; set; }
        public decimal? Commission { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }

    }
    public partial class ReinsurerDetail : Entity<ReinsurerDetail>
    {
        public ReinsurerDetail() { }
        public ReinsurerDetail(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public string ReinsurerCode { get; set; }
        public string ReinsurerName { get; set; }
        public Single? ReinsurerCommission { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }

    }
    public partial class Reinsurance : Entity<Reinsurance>
    {
        public Reinsurance() { }
        public Reinsurance(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public string TreatyCode { get; set; }
        public string TreatyName { get; set; }
        public decimal? MinTreatyCapacity { get; set; }
        public decimal? MaxTreatyCapacity { get; set; }
        public string Type { get; set; }
        public string ReinsuranceBrokerCode { get; set; }
        public DateTime CreatedOn { get; set; }
        public int?  CreatedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }

    }
    public partial class ReinsuranceTransaction : Entity<ReinsuranceTransaction>
    {
        public ReinsuranceTransaction() { }
        public ReinsuranceTransaction(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public string TreatyCode { get; set; }
        public string TreatyName { get; set; }
        public decimal? ReinsuranceAmount { get; set; }
        public decimal? ReinsuranceCommission { get; set; }
        public decimal? ReinsurancePremium { get; set; }
        public decimal? ReinsuranceCommissionPercentage { get; set; }
        public int VehicleId { get; set; }
        public int SummaryDetailId { get; set; }
        public int ReinsuranceBrokerId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }

    }
    public partial class Product : Entity<Product>
    {
        public Product() { }
        public Product(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }


    }
    public partial class PolicyInsurer : Entity<PolicyInsurer>
    {
        public PolicyInsurer() { }
        public PolicyInsurer(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public string InsurerName { get; set; }
        public string InsurerCode { get; set; }
        public string InsurerAddress { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }

    }
    public partial class VehicleCoverType : Entity<VehicleCoverType>
    {
        public VehicleCoverType() { }
        public VehicleCoverType(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public string CoverType { get; set; }

    }
    public partial class VehicleUsage : Entity<VehicleUsage>
    {
        public VehicleUsage() { }
        public VehicleUsage(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public int ProductId { get; set; }
        public string VehUsage { get; set; }
        public Single? ComprehensiveRate { get; set; }
        public decimal? MinCompAmount { get; set; }
        public Single? ThirdPartyRate { get; set; }
        public decimal? MinThirdAmount { get; set; }
        public decimal? FTPAmount { get; set; }
        public decimal? AnnualTPAmount { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }

    }
    public partial class PaymentInformation : Entity<PaymentInformation>
    {
        public PaymentInformation() { }
        public PaymentInformation(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public int SummaryDetailId { get; set; }
        public int VehicleDetailId { get; set; }
        public int PolicyId { get; set; }
        public int CustomerId { get; set; }
        public int CurrencyId { get; set; }
        public string DebitNote { get; set; }
        public int ProductId { get; set; }
        public bool DeleverLicence { get; set; }
        public string PaymentId { get; set; }
        public string InvoiceId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }

        public string InvoiceNumber { get; set; }

    }

    public partial class SmsLog : Entity<SmsLog>
    {
        public SmsLog() { }
        public SmsLog(bool defaults) : base(defaults) { }
        public int Id { get; set; }
        public string Sendto { get; set; }
        public string Body { get; set; }
        public string Response { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }

    }
    public partial class Setting : Entity<Setting>
    {
        public Setting() { }
        public Setting(bool defaults) : base(defaults) { }
        public int Id { get; set; }
        public string keyname { get; set; }
        public string value { get; set; }
        public int? ValueType { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

    }
    public partial class UserManagementView : Entity<UserManagementView>
    {
        public UserManagementView() { }
        public UserManagementView(bool defaults) : base(defaults) { }
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public string EmailAddress { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Country { get; set; }
        public string Zipcode { get; set; }
        public string State { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string PhoneNumber { get; set; }
        public string City { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }

    }
    public partial class SummaryVehicleDetail : Entity<SummaryVehicleDetail>
    {
        public SummaryVehicleDetail() { }
        public SummaryVehicleDetail(bool defaults) : base(defaults) { }
        public int Id { get; set; }
        public int SummaryDetailId { get; set; }
        public int VehicleDetailsId { get; set; }
       public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }

    }

    public partial class LicenceTicket : Entity<LicenceTicket>
    {
        public LicenceTicket() { }
        public LicenceTicket(bool defaults) : base(defaults) { }
        public int Id { get; set; }
        public string TicketNo { get; set; }
        public string PolicyNumber { get; set; }
        public int? VehicleId { get; set; }
        public bool? IsClosed { get; set; }
        public string CloseComments { get; set; }
        public string ReopenComments { get; set; }
        public string DeliveredTo { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifyDate { get; set; }
        public int? ModifyBy { get; set; }
    }

    public partial class CustomerWallet : Entity<CustomerWallet>
    {
        public CustomerWallet() { }
        public CustomerWallet(bool defaults) : base(defaults) { }
        public int Id { get; set; }
        public int? CustId { get; set; }
        public decimal? Points { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
    public partial class PolicyDocument : Entity<PolicyDocument>
    {
        public PolicyDocument() { }
        public PolicyDocument(bool defaults) : base(defaults) { }
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public string PolicyNumber { get; set; }
        public int? vehicleId { get; set; }
    }

    public partial class PolicyRenewReminderSetting : Entity<PolicyRenewReminderSetting>
    {
        public PolicyRenewReminderSetting() { }
        public PolicyRenewReminderSetting(bool defaults) : base(defaults) { }
        public int Id { get; set; }
        public int NoOfDays { get; set; }
        public int NotificationType { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public bool SMS { get; set; }
        public bool Email { get; set; }
    }
    public partial class ReminderFailed : Entity<ReminderFailed>
    {
        public ReminderFailed() { }
        public ReminderFailed(bool defaults) : base(defaults) { }
        public int Id { get; set; }
        public string EmailBody { get; set; }
        public string SendTo { get; set; }
        public string EmailSubject { get; set; }
        public int NotificationType { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }

    }

    public partial class LicenceDiskDeliveryAddress : Entity<LicenceDiskDeliveryAddress>
    {
        public LicenceDiskDeliveryAddress() { }
        public LicenceDiskDeliveryAddress(bool defaults) : base(defaults) { }
        public int Id { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public int VehicleId { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }

    }


    public partial class UniqueCustomer : Entity<UniqueCustomer>
    {
        public UniqueCustomer() { }
        public UniqueCustomer(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public int UniqueCustomerId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }


    }


    public partial class City : Entity<City>
    {
        public City() { }
        public City(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public string CityName { get; set; }
        public DateTime? CreatedOn { get; set; }

    }

    //// Second Phase Work


    public partial class ClaimNotification : Entity<ClaimNotification>
    {
        public ClaimNotification() { }
        public ClaimNotification(bool defaults) : base(defaults) { }
        public int Id { get; set; }
        public string PolicyNumber { get; set; }
        public DateTime DateOfLoss { get; set; }
        public string PlaceOfLoss { get; set; }
        public string DescriptionOfLoss { get; set; }
        public decimal EstimatedValueOfLoss { get; set; }
        public string ThirdPartyInvolvement { get; set; }
        public string ClaimantName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsRegistered { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
    }


    public partial class ServiceProvider : Entity<ServiceProvider>
    {
        public ServiceProvider() { }
        public ServiceProvider(bool defaults) : base(defaults) { }
        public int Id { get; set; }
        public int ServiceProviderType { get; set; }
        public string ServiceProviderName { get; set; }
        public string ServiceProviderContactDetails { get; set; }
        public decimal ServiceProviderFees { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
    }


    public partial class ServiceProviderType : Entity<ServiceProviderType>
    {
        public ServiceProviderType() { }
        public ServiceProviderType(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public string ProviderType { get; set; }

    }
    public partial class ClaimRegistration : Entity<ClaimRegistration>
    {
        public ClaimRegistration() { }
        public ClaimRegistration(bool defaults) : base(defaults) { }
        public int Id { get; set; }
        public string PolicyNumber { get; set; }
        //public string NameOfInsured { get; set; }

        public string PaymentDetails { get; set; }
        //public DateTime? CoverStartDate { get; set; }
        //public DateTime? CoverEndDate { get; set; }
        //public string RiskDetails { get; set; }
        public long ClaimNumber { get; set; }
        public string Checklist { get; set; }
        public DateTime? DateOfLoss { get; set; }
        public DateTime? DateOfNotifications { get; set; }
        public string PlaceOfLoss { get; set; }
        public string DescriptionOfLoss { get; set; }
        public decimal? EstimatedValueOfLoss { get; set; }
        public string ThirdPartyDamageValue { get; set; }
        public bool Claimsatisfaction { get; set; }
        public int ClaimStatus { get; set; }
        public DateTime? CreatedOn { get; set; }

        public string RejectionStatus { get; set; }


        //public DateTime? ModifyOn { get; set; }   
    }


    public partial class ClaimStatus : Entity<ClaimStatus>
    {
        public ClaimStatus() { }
        public ClaimStatus(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public string Status { get; set; }
    }

    public partial class Checklist : Entity<Checklist>
    {
        public Checklist() { }
        public Checklist(bool defaults) : base(defaults) { }
        public int Id { get; set; }
        public string ChecklistDetail { get; set; }
    }
    public partial class ClaimDetailsProvider : Entity<ClaimDetailsProvider>
    {
        public ClaimDetailsProvider() { }
        public ClaimDetailsProvider(bool defaults) : base(defaults) { }
        public int Id { get; set; }
        public int AssessorsProviderType { get; set; }
        public int ValuersProviderType { get; set; }
        public string PolicyNumber { get; set; }
        public int LawyersProviderType { get; set; }
        public int RepairersProviderType { get; set; }
        public int ClaimNumber { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }


    }
    public partial class ClaimAdjustment : Entity<ClaimAdjustment>
    {
        public ClaimAdjustment() { }
        public ClaimAdjustment(bool defaults) : base(defaults) { }
        public int Id { get; set; }
        public int? AmountToPay { get; set; }
        public int? EstimatedLoss { get; set; }
        public decimal ExcessesAmount { get; set; }
        public string PayeeBankDetails { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PayeeName { get; set; }
        public string PolicyholderName { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
        public bool? DriverIsUnder21 { get; set; }
        public bool? Islicensedless60months { get; set; }
        public bool? IsStolen { get; set; }
        public bool? IsLossInZimbabwe { get; set; }
        public bool? IsPartialLoss { get; set; }
        public string PhoneNumber { get; set; }
        public string PolicyNumber { get; set; }
        public int ClaimNumber { get; set; }
        public bool CommericalCar { get; set; }
        public bool PrivateCar { get; set; }
    }
    public partial class ClaimDocument : Entity<ClaimDocument>
    {
        public ClaimDocument() { }
        public ClaimDocument(bool defaults) : base(defaults) { }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public string PolicyNumber { get; set; }
        public int ClaimNumber { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public int ServiceProvider { get; set; }
        public int ServiceProviderName { get; set; }
        //public DateTime ModifiedOn { get; set; }
        //public int ModifiedBy { get; set; }
        public bool IsActive { get; set; }

    }
    public partial class ClaimSetting : Entity<ClaimSetting>
    {
        public ClaimSetting() { }
        public ClaimSetting(bool defaults) : base(defaults) { }
        public int Id { get; set; }
        public string KeyName { get; set; }
        public int Value { get; set; }
        public int Valuetype { get; set; }
        public int VehicleType { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsActive { get; set; }


    }


}