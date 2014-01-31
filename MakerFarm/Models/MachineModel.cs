using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakerFarm.Models
{
    public class Machine
    {
        [Key]
        [Display(Name = "Machine ID", Description = "This is the unique Machine id Makerfarm uses.")]
        public long MachineId { set; get; }

        [Display(Name = "MachineName", Description = "This is the unique hardware ID as reported by the client.")]
        public string MachineName { get; set; }

        [Display(Name = "PrinterId", Description = "This is the affiliated ID of the Printer affiliated with this hardware.")]
        public long? PrinterId { set; get; }

        [Display(Name = "Reported Status", Description = "This string is filled with reported information on what the printer is up to. First line should be Brief")]
        public string Status { set; get; }

        [Display(Name = "Idle?", Description = "The name of the print material. (IE: Navy Blue ABS)")]
        public bool idle { set; get; }

        [Display(Name = "Last Update", Description = "This is the DateTime of the last time this printer was update by the Client.")]
        public DateTime LastUpdated { get; set; }

        [Display(Name = "Active Client Support", Description = "This denotes if the printer supports the sending/cancelation of jobs Via the client.")]
        public bool ClientJobSupport { set; get; }

        public bool Enabled { get; set; }

        [ForeignKey("PrinterId")]
        public virtual Printer AffiliatedPrinter { get; set; }

        //reference to the job assigned to the current machine
        public virtual Job AssignedJob { get; set; }

        public MachineInterest GetMachineInterest()
        {
            MachineInterest M = new MachineInterest();
            M.MachineName = MachineName;
            if(AssignedJob == null){
                M.CurrentJob = 0;
            }else{
                M.CurrentJob = AssignedJob.JobId;
            }
            return M;
        }
    }
    public class MachineInterest
    {
        public string MachineName { get; set; }
        public int CurrentJob { get; set; }
    }
}