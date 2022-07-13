using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceAdminBot.Utilities
{
    public class ToDoTask
    {
        [JsonProperty(PropertyName = "id")]

        public string Id { get; set; }
        public string Task { get; set; }
        public string Email { get; set; }
        
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
