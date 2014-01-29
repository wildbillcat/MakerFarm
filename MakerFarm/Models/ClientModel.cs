using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakerFarm.Models
{
    public class Client
    {
        public int ClientId { set; get; }

        public string ClientName { set; get; }

        public string ClientUserName { set; get; }

        public string ClientAPIKey { set; get; }

        public DateTime LastUpdated { set; get; }

        public bool Enabled { set; get; }

        public virtual ICollection<ClientPermission> ClientPermissions { get; set; }
    }
}