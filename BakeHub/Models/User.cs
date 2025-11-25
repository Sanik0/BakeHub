using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BakeHub.Models
{
    [Table("users")]
    public class User
    {
        public string Firstname { get; set; }
        public int User_id { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Created_at { get; set; }
        public string Address { get; set; }
        public int Role { get; set; }
        public string ErrorMessage { get; set; }
        public HttpPostedFileBase Photo { get; set; }
        public string PhotoPath { get; set; }
        public string ExistingPhoto { get; set; }
    }
}