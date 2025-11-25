using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BakeHub.Models
{
    [Table("category")]
    public class Category
    {
        public int Category_id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Created_at { get; set; }
        public int Status { get; set; }
        public string ErrorMessage { get; set; }
        public HttpPostedFileBase Photo { get; set; }
        public string PhotoPath { get; set; }
        public string ExistingPhoto { get; set; }
    }
}