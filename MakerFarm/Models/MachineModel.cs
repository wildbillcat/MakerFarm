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

        [Display(Name = "Poison Jobs", Description = "When true this will cause RepRancher to cancel any active jobs on the Machine.")]
        public bool PoisonJobs { get; set; }

        public double? CurrentTaskProgress { get; set; }

        [Display(Name = "Pause Machine?", Description = "This registers if the machine is paused")]
        public MachinePause PauseMachine { get; set; }

        [ForeignKey("PrinterId")]
        public virtual Printer AffiliatedPrinter { get; set; }

        //reference to the job assigned to the current machine
        public virtual Job AssignedJob { get; set; }


        /*
         * Machine Glossary
         */
        //Print Related Commands
        public bool Print_Send { get; set; } //This denotes MakerFarm Job Support
        public bool Print_Cancel { get; set; }
        public bool Print_Pause { get; set; }
        public bool Print_Resume { get; set; }

        public MachineInterest GetMachineInterest()
        {
            MachineInterest M = new MachineInterest();
            M.MachineName = MachineName;
            M.PoisonJobs = PoisonJobs;
            M.PauseMachine = PauseMachine;
            
            if(AssignedJob == null){
                M.CurrentJob = 0;
                M.PreviouslyCollected = false; 
                M.EstMaterialUse = 0;
                M.EstToolpathTime = 0;
                M.PrintFileName = null;
            }else{
                M.CurrentJob = AssignedJob.JobId;
                M.PreviouslyCollected = AssignedJob.started;
                M.EstMaterialUse = AssignedJob.AffiliatedPrint.EstMaterialUse;
                M.EstToolpathTime = AssignedJob.AffiliatedPrint.EstToolpathTime;
                M.PrintFileName = string.Concat(AssignedJob.AffiliatedPrint.PrintId.ToString(), "_", AssignedJob.AffiliatedPrint.FileName);
            }
            return M;
        }
    }

    public class MachineInterest
    {
        public string MachineName { get; set; }
        public bool PoisonJobs { get; set; }
        public bool PreviouslyCollected { get; set; }
        public MachinePause PauseMachine { get; set; }

        public int CurrentJob { get; set; }
        public string PrintFileName { get; set; }
        public int EstToolpathTime { get; set; } /* Estimated amount of time (in minutes) to complete the print job from the processing software */
        public double EstMaterialUse { get; set; } /* The print cost originally estimated by the print software (Ounces) */
    }

    public class MachineStatusUpdate
    {
        public string MachineName { get; set; }
        public string MachineStatus { get; set; }
        public double? CurrentTaskProgress { get; set; }
        public double MachinePaused { get; set; }
    }

    public class RancherCommandGlossary
    {
        public string MachineName { get; set; }
        //Print Related Commands
        public bool Print_Send { get; set; } //This denotes MakerFarm Job Support
        public bool Print_Cancel { get; set; }
        public bool Print_Pause { get; set; }
        public bool Print_Resume { get; set; }
    }

    //Printer can be actively printing, paused due to MakerFarm, have a Reume Command queued by MakerFarm, or have been paused at the machine. 
    //By Default the Machine would be Active printing.
    public enum MachinePause
    {
        ActivePrinting, MakerfarmPause, MakerfarmResume, PausedAtMachine
    }
}