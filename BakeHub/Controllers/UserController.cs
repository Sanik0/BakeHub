using BakeHub.Data;
using BakeHub.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace BakeHub.Controllers
{
    public class UserController : Controller
    {
        private MySQLContext db = new MySQLContext();

        public ActionResult AddUser()
        {
            return View(new User());
        }

        //======================================ADD USER====================================

        // Hash the password
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        [HttpPost]
        public ActionResult AddUser(User model)
        {
            using (var conn = db.Connection)
            {
                conn.Open();

                // 1️⃣ Check if email exists
                string checkQuery = "SELECT COUNT(*) FROM users WHERE email=@Email";
                using (var cmd = new MySqlCommand(checkQuery, (MySqlConnection)conn))
                {
                    cmd.Parameters.AddWithValue("@Email", model.Email);

                    int count = (int)(long)cmd.ExecuteScalar();
                    if (count > 0)
                    {
                        model.ErrorMessage = "Email already exists.";
                        model.Email = ""; // Clear email only
                        return View(model); // repopulate form
                    }
                }

                // Check is passwords match
                if (model.Password != model.ConfirmPassword)
                {
                    model.ErrorMessage = "Passwords do not match.";
                    model.Password = "";
                    model.ConfirmPassword = "";
                    return View(model);
                }

                //Hash before saving
                string hashedPassword = HashPassword(model.Password);

                // Save User image
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
                string insertQuery = @"INSERT INTO users
                (firstname, lastname, email, contact, password, address, role, image)
                VALUES (@Firstname, @Lastname, @Email, @Contact, @Password, @Address, @Role, @Photo)";

                using (var cmd = new MySqlCommand(insertQuery, (MySqlConnection)conn))
                {
                    cmd.Parameters.AddWithValue("@Firstname", model.Firstname);
                    cmd.Parameters.AddWithValue("@Lastname", model.Lastname);
                    cmd.Parameters.AddWithValue("@Email", model.Email);
                    cmd.Parameters.AddWithValue("@Contact", model.Contact);
                    cmd.Parameters.AddWithValue("@Password", hashedPassword);
                    cmd.Parameters.AddWithValue("@Address", model.Address);
                    cmd.Parameters.AddWithValue("@Role", model.Role);
                    cmd.Parameters.AddWithValue("@Photo", savedFileName ?? "");

                    cmd.ExecuteNonQuery();
                }
            }
            TempData["SuccessMessage"] = "User added successfully!";
            return RedirectToAction("Users", "User");
        }

        //======================================USER TABLE====================================
        public ActionResult Users(int? roleFilter)
        {
            List<User> usersList = new List<User>();

            using (var conn = db.Connection)
            {
                conn.Open();

                // Build query based on filter
                string query = "SELECT * FROM users";
                if (roleFilter.HasValue)
                {
                    query += " WHERE role = @Role";
                }

                using (var cmd = new MySqlCommand(query, (MySqlConnection)conn))
                {
                    if (roleFilter.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@Role", roleFilter.Value);
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            usersList.Add(new User
                            {
                                Firstname = reader["firstname"] == DBNull.Value ? "" : reader["firstname"].ToString(),
                                Lastname = reader["lastname"] == DBNull.Value ? "" : reader["lastname"].ToString(),
                                Email = reader["email"] == DBNull.Value ? "" : reader["email"].ToString(),
                                Contact = reader["contact"] == DBNull.Value ? "" : reader["contact"].ToString(),
                                Address = reader["address"] == DBNull.Value ? "" : reader["address"].ToString(),
                                Role = reader["role"] == DBNull.Value ? 0 : Convert.ToInt32(reader["role"]),
                                Created_at = reader["created_at"] == DBNull.Value ? "" : reader["created_at"].ToString(),
                                User_id = reader["user_id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["user_id"]),
                                ExistingPhoto = reader["image"] == DBNull.Value ? "" : reader["image"].ToString()
                            });
                        }
                    }
                }
            }

            // Pass the current filter to the view
            ViewBag.CurrentFilter = roleFilter;

            return View(usersList);
        }
        //======================================EDIT USER====================================
        public ActionResult EditUser(int? id)
        {
            // Check if url has an id
            if (id == null || id == 0)
            {
                TempData["SuccessMessage"] = "User not found.";
                return RedirectToAction("Users", "User");
            }

            User model = null;

            using (var conn = db.Connection)
            {
                conn.Open();

                string query = "SELECT * FROM users WHERE user_id=@User_d";
                using (var cmd = new MySqlCommand(query, (MySqlConnection)conn))
                {
                    cmd.Parameters.AddWithValue("@User_d", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model = new User
                            {
                                Firstname = reader["firstname"] == DBNull.Value ? "" : reader["firstname"].ToString(),
                                Lastname = reader["lastname"] == DBNull.Value ? "" : reader["lastname"].ToString(),
                                Email = reader["email"] == DBNull.Value ? "" : reader["email"].ToString(),
                                Contact = reader["contact"] == DBNull.Value ? "" : reader["contact"].ToString(),
                                Address = reader["address"] == DBNull.Value ? "" : reader["address"].ToString(),
                                Role = reader["role"] == DBNull.Value ? 0 : Convert.ToInt32(reader["role"]),
                                Created_at = reader["created_at"] == DBNull.Value ? "" : reader["created_at"].ToString(),
                                User_id = reader["user_id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["user_id"]),
                                ExistingPhoto = reader["image"] == DBNull.Value ? "" : reader["image"].ToString()
                            };
                        }
                    }
                }
            }

            // Check is if user id has any matches in the database
            if (model == null)
            {
                TempData["SuccessMessage"] = "User not found.";
                return RedirectToAction("Users", "User");
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult EditUser(User model)
        {
            using (var conn = db.Connection)
            {
                conn.Open();

                // 1️⃣ Check if email exists for another user
                string checkQuery = "SELECT COUNT(*) FROM users WHERE email=@Email AND user_id != @User_id";
                using (var cmd = new MySqlCommand(checkQuery, (MySqlConnection)conn))
                {
                    cmd.Parameters.AddWithValue("@Email", model.Email);
                    cmd.Parameters.AddWithValue("@User_id", model.User_id);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    if (count > 0)
                    {
                        model.ErrorMessage = "Email already exists.";
                        return View(model); 
                    }
                }

                // 2️⃣ Check if passwords match (only if user entered something)
                string hashedPassword = null;
                if (!string.IsNullOrEmpty(model.Password) || !string.IsNullOrEmpty(model.ConfirmPassword))
                {
                    if (model.Password != model.ConfirmPassword)
                    {
                        model.ErrorMessage = "Passwords do not match.";
                        model.Password = "";
                        model.ConfirmPassword = "";
                        return View(model);
                    }

                    hashedPassword = HashPassword(model.Password);
                }

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
                string updateQuery = @"UPDATE users SET 
                                firstname=@Firstname,
                                lastname=@Lastname,
                                email=@Email,
                                contact=@Contact,
                                address=@Address,
                                role=@Role,
                                image=@Photo"
                                    + (hashedPassword != null ? ", password=@Password" : "") +
                                    " WHERE user_id=@User_id";

                using (var cmd = new MySqlCommand(updateQuery, (MySqlConnection)conn))
                {
                    cmd.Parameters.AddWithValue("@Firstname", model.Firstname);
                    cmd.Parameters.AddWithValue("@Lastname", model.Lastname);
                    cmd.Parameters.AddWithValue("@Email", model.Email);
                    cmd.Parameters.AddWithValue("@Contact", model.Contact);
                    cmd.Parameters.AddWithValue("@Address", model.Address);
                    cmd.Parameters.AddWithValue("@Role", model.Role);
                    cmd.Parameters.AddWithValue("@Photo", savedFileName ?? "");
                    cmd.Parameters.AddWithValue("@User_id", model.User_id);

                    if (hashedPassword != null)
                        cmd.Parameters.AddWithValue("@Password", hashedPassword);

                    cmd.ExecuteNonQuery();
                }
            }

            TempData["SuccessMessage"] = "User updated successfully!";
            return RedirectToAction("Users", "User");
        }

        //======================================DELETE USER====================================
        [HttpPost]
        public ActionResult DeleteUser(int id)
        {
            using (var conn = db.Connection)
            {
                conn.Open();

                string query = "DELETE FROM users WHERE user_id=@id";

                using (var cmd = new MySqlCommand(query, (MySqlConnection)conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    int rows = cmd.ExecuteNonQuery();

                    if (rows == 0)
                    {
                        TempData["SuccessMessage"] = "User not found.";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "User deleted successfully!";
                    }
                }
            }

            return RedirectToAction("Users", "User");
        }
        //======================================VIEW USER====================================

        public ActionResult EditUser(int id)
        {
            return View();
        }
    }

}