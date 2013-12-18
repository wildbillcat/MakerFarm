﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MakerFarm.Models
{
    public class Print
    {
        public long PrintId { get; set; } /* Generic Id for the Print Job */

        [Display(Name = "File Name", Description = "The file name of the print submitted")]
        public string FileName { get; set; } /* The file name of the print submitted */

        [Display(Name = "User Name", Description = "User Name of the person whom submitted the print")]
        public string UserName { get; set; } /* User ID of the person whom submitted the print */

        [Display(Name = "Material", Description = "This is the Material(s) that should be used for each printer.")]
        public string MaterialIds { get; set; } /* This is an array listing the MaterialIDs of the types of materials requested for the printer job */

        [Display(Name = "Estimated Material Usage", Description = "Estimated amount of Material usage given by the software")]
        public double EstMaterialUse { get; set; } /* The print cost originally estimated by the print software */

        [DataType(DataType.Date)]
        [Display(Name = "Submission Time", Description = "Time the Submission was made")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yy H:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime SubmissionTime { get; set; } /* This is the time the print is submitted to the application */


        [Range(1, int.MaxValue, ErrorMessage = "Value Should be between 1 and 2,147,483,647")]
        [Display(Name = "Estimated Print Time (Minutes)", Description = "Estimated amount of time print will take in minutes")]
        public int EstToolpathTime { get; set; } /* Estimated amount of time (in minutes) to complete the print job from the processing software */

        [Display(Name = "Number of Authorized Attempts", Description = "Number of print attempts authorized to be tried at your cost.")]
        public int AuthorizedAttempts { get; set; } /* Number of attempts that are requested to be attempted in at cost to the user. */

        [Display(Name = "Printer Type", Description = "The Type of printer this print is made for")]
        public int PrinterTypeId { get; set; } /* ID number of the printer type the file is meant for */

        [Display(Name = "Staff Assisted Print", Description = "Denotes if a Printing staff member assisted with the submission of this print")]
        public bool StaffAssistedPrint { get; set; } /* Denotes if a staff member assited with the print submission */

        public string Comment { get; set; } /*Commonly used to denote on multiple extruder machines which extruder gets */

        [Display(Name = "Flagged Print", Description = "Denotes if a Print has been flagged by a staff member for some reason")]
        public bool FlaggedPrint { get; set; }

        [Display(Name = "Flagged Comment", Description = "This comment is set to note why a print was Flagged for future reference")]
        public string FlaggedComment { get; set; } /*Commonly used to denote on multiple extruder machines which extruder gets */

        public virtual PrinterType PrinterType { get; set; }
        public virtual ICollection<PrintEvent> PrintEvents { get; set; }
    }
        
}