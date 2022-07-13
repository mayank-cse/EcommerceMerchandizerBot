using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceAdminBot
{
    public class MarketDetails
    {
        public List<ProductM> ProductList = new List<ProductM>();
    }

    public class ProductM
    {
        public string Email { get; set; }
        //public string ID { get; set; }
        public string SName { get; set; }
        public string location { get; set; }
        public string Photo { get; set; }
        //public string Company { get; set; }
        public string ImageURL { get; set; }
        public string Category { get; set; }
        public string textmessage { get; set; }
        public string informationCategory { get; set; }
    }
}

