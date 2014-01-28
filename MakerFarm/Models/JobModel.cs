using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakerFarm.Models
{
    public class Job
    {
        
        public int JobId { get; set; } //This is the internal Job id for Makerfarm, not the JobID for Conveyor

        public long PrintId { get; set; } /* Generic Id for the Print Job */

        [Display(Name = "Assigned Unique Hardware Device ID", Description = "This is the unique hardware ID as reported by the client.")]
        public string MachineId;

        [Display(Name = "Reported Status", Description = "This string is filled with reported information on what the job is up to.")]
        public string Status { set; get; }

        [Display(Name = "Job has been fetched by Client", Description = "This notes if a client has fetched and started the job")]
        public bool started { set; get; }

        [Display(Name = "Job has been completed", Description = "The name of the print material. (IE: Navy Blue ABS)")]
        public bool complete { set; get; }

        [Display(Name = "Last Update", Description = "This is the DateTime of the last time this printer was update by the Client.")]
        public DateTime LastUpdated { get; set; }

        [ForeignKey("PrintId")]
        public Printer AffiliatedPrint { get; set; }

        [ForeignKey("MachineId")]
        public Printer AffiliatedMachine { get; set; }

    }
}