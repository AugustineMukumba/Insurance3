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
        ThirdParty = 2
    }
    public enum eExcessType
    {
        Percentage = 1,
        FixedAmount = 2
    }
    public enum ePaymentTerm
    {
        Annual = 1,
        Quarterly = 2,
        Termly = 3,
        Monthly = 4
    }
}
