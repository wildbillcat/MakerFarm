using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakerFarm.Models
{
    public class Bill
    {
        public long BillId { get; set; } /* Generic Id for the Bill */
        
        [Display(Name = "User Name", Description = "User Name of the person whom charged the print")]
        public string UserName { get; set; } 

        [DataType(DataType.Date)]
        [Display(Name = "Billing Time", Description = "Time the Billing was made")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yy H:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime BillingTime { get; set; } 

        [Range(0, double.MaxValue, ErrorMessage = "Value Should be between 1 and 2,147,483,647")]
        [Display(Name = "Total Billing amount", Description = "The total amount of money charged to the user")]
        public double TotalBillingAmount { get; set; } 

        [Display(Name = "Print Event ID", Description = "The Print Event ID that this billing is associated with.")]
        public int PrintEventId { get; set; } 

        [Display(Name = "Print ID", Description = "The Print ID assiciated with the bill")]
        public int PrintId { get; set; } 

        [Display(Name = "Comment", Description = "This holds a comment about the billing (Such as an explanation of the Billing)")]
        public string Comment { get; set; }

        public virtual Print Print { get; set; }

        public virtual PrintEvent PrintEvent { get; set; }
    }
}