using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakerFarm.Models
{
    public class Client
    {
        public int ClientId { set; get; }

        public string ClientName { set; get; }

        public string ClientSemmetricAPIKey { set; get; }
    }
}