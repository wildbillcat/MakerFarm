using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakerFarm.Models
{
    public class ClientPermission
    {
        public long ClientPermissionId { set; get; }

        [Display(Name = "Get Information Permission", Description = "This denotes if the client is allowed to read the Hardware's information (and recieve commands for the hardware).")]
        public bool GetInformation { get; set; }

        [Display(Name = "Set Information Permission", Description = "This denotes if the client is allowed to update the Hardware's inforamtion.")]
        public bool SetInformation { get; set; }

        public virtual Client Client { get; set; }

        public virtual Machine Machine { get; set; }

        
    }
}