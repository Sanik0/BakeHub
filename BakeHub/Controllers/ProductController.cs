using BakeHub.Data;
using BakeHub.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BakeHub.Controllers
{
    public class ProductController : Controller

    {

        private MySQLContext db = new MySQLContext();
        // GET: Product
        //======================================PRODUCT TABLE====================================
        public ActionResult Products(int? statusFilter)
        {
            List<Product> productsList = new List<Product>();

            using (var conn = db.Connection)
            {
                conn.Open();

                // Build query based on filter
                string query = "SELECT * FROM products";
                if (statusFilter.HasValue)
                {
                    query += " WHERE status = @Status";
                }

                using (var cmd = new MySqlCommand(query, (MySqlConnection)conn))
                {
                    if (statusFilter.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@Status", statusFilter.Value);
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            productsList.Add(new Product
                            {
                                Name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString(),
                                Price = reader["price"] == DBNull.Value ? 1 : Convert.ToInt32(reader["price"]),
                                Stock = reader["stock"] == DBNull.Value ? 1 : Convert.ToInt32(reader["stock"]),
                                Status = reader["status"] == DBNull.Value ? 1 : Convert.ToInt32(reader["status"]),
                                Created_at = reader["created_at"] == DBNull.Value ? "" : reader["created_at"].ToString(),
                                Category_id = reader["category_id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["category_id"]),
                                ExistingPhoto = reader["image"] == DBNull.Value ? "" : reader["image"].ToString()
                            });
                        }
                    }
                }
            }

            // Pass the current filter to the view
            ViewBag.CurrentFilter = statusFilter;

            return View(productsList);
        }

        //======================================ADD Product====================================
        public ActionResult AddProduct()
        {
            List<SelectListItem> categories = new List<SelectListItem>();

            using (var conn = db.Connection)
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT category_id, title FROM categories";

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    categories.Add(new SelectListItem
                    {
                        Value = reader["category_id"].ToString(),
                        Text = reader["title"].ToString()
                    });
                }
            }

            ViewBag.Categories = categories;
            return View(new Product());
        }

        [HttpPost]
        public ActionResult AddProduct(Product model)
        {
            using (var conn = db.Connection)
            {
                conn.Open();

                // Save Product image
                string savedFileName = null;

                if (model.Photo != null && model.Photo.ContentLength > 0)
                {
                    string folder = Server.MapPath("~/Content/Images/");
                    Directory.CreateDirectory(folder); // makes sure folder exists

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Photo.FileName);
                    string fullPath = Path.Combine(folder, fileName);

                    model.Photo.SaveAs(fullPath);

                    // Save only the relative path in the database
                    savedFileName = "/Content/Images/" + fileName;
                }

                // 2️⃣ Insert
                string insertQuery = @"INSERT INTO products
                (name, price, image, stock, category_id, status)
                VALUES (@Name, @Price, @Photo, @Stock, @Category_id, @Status)";

                using (var cmd = new MySqlCommand(insertQuery, (MySqlConnection)conn))
                {
                    cmd.Parameters.AddWithValue("@Name", model.Name);
                    cmd.Parameters.AddWithValue("@Price", model.Price);
                    cmd.Parameters.AddWithValue("@Category_id", model.Category_id);
                    cmd.Parameters.AddWithValue("@Stock", model.Stock);
                    cmd.Parameters.AddWithValue("@Status", model.Status);
                    cmd.Parameters.AddWithValue("@Photo", savedFileName ?? "");

                    cmd.ExecuteNonQuery();
                }
            }
            TempData["SuccessMessage"] = "Product added successfully!";
            return RedirectToAction("products", "product");
        }


    }
}