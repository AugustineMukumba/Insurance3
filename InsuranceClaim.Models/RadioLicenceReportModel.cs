using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
 public class RadioLicenceReportModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Policy_Number { get; set; }
        public string Transaction_date { get; set; }
        public decimal? RadioLicenseCost { get; set; }
    }
    public class ListRadioLicenceReport
    {
        public List<RadioLicenceReportModel> RadioLicence { get; set; }
    }

}
