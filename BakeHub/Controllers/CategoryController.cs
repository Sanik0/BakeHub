using BakeHub.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using BakeHub.Data;

namespace BakeHub.Controllers
{
    public class CategoryController : Controller
    {
        private MySQLContext db = new MySQLContext();
        // GET: Category
        //======================================Category TABLE====================================
        public ActionResult Categories(int? statusFilter)
        {
            List<Category> categoriesList = new List<Category>();

            using (var conn = db.Connection)
            {
                conn.Open();

                // Build query based on filter
                string query = "SELECT * FROM categories";
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
                            categoriesList.Add(new Category
                            {
                                Title = reader["title"] == DBNull.Value ? "" : reader["title"].ToString(),
                                Description = reader["description"] == DBNull.Value ? "" : reader["description"].ToString(),
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

            return View(categoriesList);
        }

        public ActionResult AddCategory()
        {
            return View(new Category());
        }

        //======================================ADD Category====================================
        [HttpPost]
        public ActionResult AddCategory(Category model)
        {
            using (var conn = db.Connection)
            {
                conn.Open();

                // Save Category image
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
                string insertQuery = @"INSERT INTO categories
                (title, description, image, status)
                VALUES (@Title, @Description, @Photo, @Status)";

                using (var cmd = new MySqlCommand(insertQuery, (MySqlConnection)conn))
                {
                    cmd.Parameters.AddWithValue("@Title", model.Title);
                    cmd.Parameters.AddWithValue("@Description", model.Description);
                    cmd.Parameters.AddWithValue("@Status", model.Status);
                    cmd.Parameters.AddWithValue("@Photo", savedFileName ?? "");

                    cmd.ExecuteNonQuery();
                }
            }
            TempData["SuccessMessage"] = "Category added successfully!";
            return RedirectToAction("Categories", "Category");
        }

        //======================================DELETE CATEGORY====================================
        [HttpPost]
        public ActionResult DeleteCategory(int id)
        {
            using (var conn = db.Connection)
            {
                conn.Open();

                string query = "DELETE FROM categories WHERE category_id=@id";

                using (var cmd = new MySqlCommand(query, (MySqlConnection)conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    int rows = cmd.ExecuteNonQuery();

                    if (rows == 0)
                    {
                        TempData["SuccessMessage"] = "Category not found.";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "Category deleted successfully!";
                    }
                }
            }

            return RedirectToAction("categories", "category");
        }

        //======================================EDIT CATEGORY====================================
        public ActionResult EditCategory(int? id)
        {
            // Check if url has an id
            if (id == null || id == 0)
            {
                TempData["SuccessMessage"] = "Category not found.";
                return RedirectToAction("categories", "category");
            }

            Category model = null;

            using (var conn = db.Connection)
            {
                conn.Open();

                string query = "SELECT * FROM categories WHERE category_id=@Category_d";
                using (var cmd = new MySqlCommand(query, (MySqlConnection)conn))
                {
                    cmd.Parameters.AddWithValue("@Category_d", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model = new Category
                            {
                                Title = reader["title"] == DBNull.Value ? "" : reader["title"].ToString(),
                                Description = reader["description"] == DBNull.Value ? "" : reader["description"].ToString(),
                                Status = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]),
                                Created_at = reader["created_at"] == DBNull.Value ? "" : reader["created_at"].ToString(),
                                Category_id = reader["category_id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["category_id"]),
                                ExistingPhoto = reader["image"] == DBNull.Value ? "" : reader["image"].ToString()
                            };
                        }
                    }
                }
            }

            // Check is if category id has any matches in the database
            if (model == null)
            {
                TempData["SuccessMessage"] = "Category not found.";
                return RedirectToAction("categories", "category");
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult EditCategory(Category model)
        {
            using (var conn = db.Connection)
            {
                conn.Open();


                // 3️⃣ Handle photo update
                string savedFileName = model.ExistingPhoto; // keep old photo if nothing uploaded
                if (model.Photo != null && model.Photo.ContentLength > 0)
                {
                    string folder = Server.MapPath("~/Content/Images/");
                    Directory.CreateDirectory(folder);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Photo.FileName);
                    string fullPath = Path.Combine(folder, fileName);

                    model.Photo.SaveAs(fullPath);
                    savedFileName = "/Content/Images/" + fileName;
                }

                // 4️⃣ Update query
                string updateQuery = @"UPDATE categories SET 
                                title=@Title,
                                description=@Description,
                                status=@Status,
                                image=@Photo WHERE category_id=@Category_id";

                using (var cmd = new MySqlCommand(updateQuery, (MySqlConnection)conn))
                {
                    cmd.Parameters.AddWithValue("@Title", model.Title);
                    cmd.Parameters.AddWithValue("@Description", model.Description);
                    cmd.Parameters.AddWithValue("@Status", model.Status);
                    cmd.Parameters.AddWithValue("@Photo", savedFileName ?? "");
                    cmd.Parameters.AddWithValue("@Category_id", model.Category_id);
                    cmd.ExecuteNonQuery();
                }
            }

            TempData["SuccessMessage"] = "Category updated successfully!";
            return RedirectToAction("categories", "category");
        }
    }
}