using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace InsuranceClaim.Models
{
    public class ReceiptModuleModel
    {
   
        public int Id { get; set; }
        public int PolicyId { get; set; }
        public int CustomerId { get; set; }
        public string PolicyNumber { get; set; }

        public string PolicyNo { get; set; }
        public string CustomerName { get; set; }
        public string InvoiceNumber { get; set; }
        public int? PaymentMethodId { get; set; }
        public decimal? AmountDue {get;set;}

        public decimal? AmountPaid { get; set; }
        public string Balance { get; set; }
        public DateTime DatePosted { get; set; }
        public string TransactionReference { get; set; }

        public int SummaryDetailId { get; set; }

        public int CreatedBy { get; set; }


    }

}
