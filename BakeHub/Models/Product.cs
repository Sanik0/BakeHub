using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BakeHub.Models
{
    public class Product
    {
        public int Product_id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Created_at { get; set; }
        public int Status { get; set; }
        public int Stock { get; set; }
        public int Category_id { get; set; }
        public string Title { get; set; }
        public string ErrorMessage { get; set; }
        public HttpPostedFileBase Photo { get; set; }
        public string PhotoPath { get; set; }
        public string ExistingPhoto { get; set; }
    }
}