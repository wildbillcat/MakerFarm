using System;
using System.ComponentModel.DataAnnotations;

namespace MakerFarm.Models
{

    public enum PrintEventType
    {
        PRINT_START, PRINT_FAILURE_FILE, PRINT_FAILURE_MACHINE, PRINT_COMPLETED, PRINT_CANCELED
    }

    public class PrintEvent
    {

        public long PrintEventId { get; set; } /* Generic Event ID to distinguish between events */

        [Display(Name = "Event Type", Description = "The Event Type distinguises what type of events has occured")]
        public PrintEventType EventType { get; set; } /* The Event Type ID (One of the Staticly Defined Types) */

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Event Timestamp", Description = "This is the TimeStamp of when the event was created")]
        public DateTime EventTimeStamp { get; set; } /* Time the Event was Registered */

        [Display(Name = "Material Used", Description = "This is the amount of Material Used by the event")]
        public double MaterialUsed { get; set; } /* Amount of Material Used by event */

        [Display(Name = "Printer", Description = "This Identifies the Specific Printer associated with the event")]
        public long PrinterId { get; set; } /* ID number of the printer that event was associated with. */

        [Display(Name = "User Name", Description = "User Whom the action is affiliated with")]
        public string UserName { get; set; } /* User ID of who registered the event */

        [Display(Name = "Print", Description = "The Print associated with the Print Event")]
        public long PrintId { get; set; }

        public virtual Print Print { get; set; }

        public virtual Printer Printer { get; set; }

        


       
    }
       
}