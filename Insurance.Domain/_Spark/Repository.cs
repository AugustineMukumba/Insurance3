
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
    // # Generated on 10/11/2016 9:10:49 AM
    // #
    // # Edits to this file may cause incorrect behavior and will be lost
    // # if the code is regenerated.
    // #
    // ############################################################################

    // Insurance Context. hosts all repositories

    public static class InsuranceContext
    {
        static Db db = new InsuranceDb();

        // entity specific repositories
        public static string ConnectionString { get { return db.connectionString; } }
        public static Currencies Currencies { get { return new Currencies(); } }
        public static AgentCommissions AgentCommissions { get { return new AgentCommissions(); } }
        public static BusinessSources BusinessSources { get { return new BusinessSources(); } }
        public static CoverTypes CoverTypes { get { return new CoverTypes(); } }
        public static Customers Customers { get { return new Customers(); } }
        public static LicenseDeliveries LicenseDeliveries { get { return new LicenseDeliveries(); } }
        public static LoyaltyDetails LoyaltyDetails { get { return new LoyaltyDetails(); } }
        public static PaymentMethods PaymentMethods { get { return new PaymentMethods(); } }
        public static PaymentTerms PaymentTerms { get { return new PaymentTerms(); } }
        public static PolicyDetails PolicyDetails { get { return new PolicyDetails(); } }
        public static PolicyStatuses PolicyStatuses { get { return new PolicyStatuses(); } }
        public static SummaryDetails SummaryDetails { get { return new SummaryDetails(); } }
        public static VehicleDetails VehicleDetails { get { return new VehicleDetails(); } }
        public static VehicleMakes VehicleMakes { get { return new VehicleMakes(); } }
        public static VehicleModels VehicleModels { get { return new VehicleModels(); } }
        public static BasicExcesses BasicExcesses { get { return new BasicExcesses(); } }
        public static AgeExcesses AgeExcesses { get { return new AgeExcesses(); } }
        public static SpecificExcesses SpecificExcesses { get { return new SpecificExcesses(); } }

        public static Reinsurances Reinsurances { get { return new Reinsurances(); } }
        public static ReinsuranceBrokers ReinsuranceBrokers { get { return new ReinsuranceBrokers(); } }
        public static ReinsuranceTransactions ReinsuranceTransactions { get { return new ReinsuranceTransactions(); } }
        public static ReinsurerDetails ReinsurerDetails { get { return new ReinsurerDetails(); } }
        public static Products Products { get { return new Products(); } }
        public static PolicyInsurers PolicyInsurers { get { return new PolicyInsurers(); } }
        public static VehicleCoverTypes VehicleCoverTypes { get { return new VehicleCoverTypes(); } }
        public static VehicleUsages VehicleUsages { get { return new VehicleUsages(); } }

        public static PaymentInformations PaymentInformations { get { return new PaymentInformations(); } }

        // general purpose operations

        public static void Execute(string sql, params object[] parms) { db.Execute(sql, parms); }
        public static IEnumerable<dynamic> Query(string sql, params object[] parms) { return db.Query(sql, parms); }
        public static object Scalar(string sql, params object[] parms) { return db.Scalar(sql, parms); }

        public static DataSet GetDataSet(string sql, params object[] parms) { return db.GetDataSet(sql, parms); }
        public static DataTable GetDataTable(string sql, params object[] parms) { return db.GetDataTable(sql, parms); }
        public static DataRow GetDataRow(string sql, params object[] parms) { return db.GetDataRow(sql, parms); }
       

    }
}
