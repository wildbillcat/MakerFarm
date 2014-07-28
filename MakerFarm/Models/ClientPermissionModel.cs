using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakerFarm.Models
{
    public class ClientPermission
    {
        public long ClientPermissionId { set; get; }

        public virtual Client Client { get; set; }

        public virtual Machine Machine { get; set; }

        
    }
}