	
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
    // # Generated on 9/18/2016 7:58:26 PM
    // #
    // # Edits to this file may cause incorrect behavior and will be lost
    // # if the code is regenerated.
    // #
    // ############################################################################
	
	// Insurance Db

	public partial class InsuranceDb : Db
	{
#if SVDEV
        public InsuranceDb() : base("Insurance-d") { }
#elif SVSTAGE
        public InsuranceDb() : base("Insurance-s") { }
#else
        public InsuranceDb() : base("Insurance") {}
#endif
    }
}
