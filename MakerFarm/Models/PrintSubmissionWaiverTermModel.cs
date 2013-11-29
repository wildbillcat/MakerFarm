using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MakerFarm.Models
{
    public class PrintSubmissionWaiverTerm
    {
        public int PrintSubmissionWaiverTermId { get; set; }
        public bool Enabled { get; set; }
        public string WaiverText { get; set; }
    }
}