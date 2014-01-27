using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MakerFarm.Models
{
    public class PrintSubmissionWaiverTerm
    {
        public int PrintSubmissionWaiverTermId { get; set; }

        [Display(Name = "Enable Waiver Term")]
        public bool Enabled { get; set; }

        [Display(Name = "Waiver Text")]
        public string WaiverText { get; set; }

        [Display(Name = "Display Term to Internal Users")]
        public bool ShowInternalUsers {get; set;}

        [Display(Name = "Display Term to External Users")]
        public bool ShowExternalUsers { get; set;  }
    }
}