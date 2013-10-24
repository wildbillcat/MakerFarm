using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MakerFarm.Models
{
    public class PrintModel
    {
        long ID { get; set; }
        string FileName { get; set; }
        string UserID { get; set; }
        double PrintCost { get; set; }
        DateTime SubmissionTime { get; set; }
        DateTime PrintTime { get; set; }
        DateTime CompletionTime { get; set; }
    }
}