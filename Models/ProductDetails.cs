using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceAdminBot
{
    public class ProductDetails
    {
        public List<Product> ProductList = new List<Product>();
    }

    public class Product
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public string ImageURL { get; set; }
        public string Category { get; set; }
    }
}
