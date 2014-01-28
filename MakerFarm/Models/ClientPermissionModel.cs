using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakerFarm.Models
{
    public class ClientPermission
    {
        public long ClientPermissionId { set; get; }

        [Display(Name = "Unique Client ID", Description = "This is the unique Client ID as reported by the client.")]
        public int ClientId { set; get; }

        [Display(Name = "Unique Hardware Device ID", Description = "This is the unique hardware ID as reported by the client.")]
        public string HardwareDeviceId;

        [Display(Name = "Get Information Permission", Description = "This denotes if the client is allowed to read the Hardware's information (and recieve commands for the hardware).")]
        public bool GetInformation { get; set; }

        [Display(Name = "Set Information Permission", Description = "This denotes if the client is allowed to update the Hardware's inforamtion.")]
        public bool SetInformation { get; set; }
    }
}