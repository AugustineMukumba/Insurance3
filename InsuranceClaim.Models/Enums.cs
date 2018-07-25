using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public enum eCoverType
    {
        Comprehensive = 1,
        ThirdParty = 2,
        FullThirdParty = 3
    }
    public enum eExcessType
    {
        Percentage = 1,
        FixedAmount = 2
    }
    public enum ePaymentTerm
    {
        Annual = 1,
        //Monthly = 2,
        Quarterly = 3,
        Termly = 4
    }

    public enum eSettingValueType
    {
        percentage = 1,
        amount = 2
    }
}
