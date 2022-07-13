using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceAdminBot.Models
{
    public class MarketDBDetails
    {
        [JsonProperty(PropertyName = "id")]
        public string Email { get; set; }
        /*public string StoreName { get; set; }
        public int Price { get; set; }
        public string Image { get; set; }
        public string Category { get; set; }
        
         public string Email { get; set; }
        */
         //public string Id { get; set; }
         public string StoreName { get; set; }
         public string Location { get; set; }

         public string Photo { get; set; }
         //public string Company { get; set; }
         public string Image { get; set; }
         public string Category { get; set; }
         public string TextMessage { get; set; }
        public string informationCategory { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}