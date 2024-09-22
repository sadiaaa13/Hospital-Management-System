using Demo.Models;
using Demo.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Demo.Controllers
{
    public class AdminAccountController : Controller
    {
        private NewHospitalEntities db = new NewHospitalEntities();
        //private ApplicationDbContext _context;

        //public AdminController()
        //{
        //    _context = new ApplicationDbContext();
        //}

        // GET: Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public ActionResult Login(AdminLoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Use the updated connection string from web.config
                string mainconn = ConfigurationManager.ConnectionStrings["NewHospitalSql"].ConnectionString;
                using (SqlConnection sqlconn = new SqlConnection(mainconn))
                {
                    // SQL query to select admin by email and password
                    string sqlquery = "SELECT admin_id, admin_name, admin_email FROM [dbo].[Admin] WHERE admin_email = @Email AND admin_password = @Password";

                    using (SqlCommand sqlcomm = new SqlCommand(sqlquery, sqlconn))
                    {
                        // Add parameters to prevent SQL injection
                        sqlcomm.Parameters.AddWithValue("@Email", model.Email);
                        sqlcomm.Parameters.AddWithValue("@Password", model.Password);

                        sqlconn.Open();

                        // Execute the query and read results
                        SqlDataReader reader = sqlcomm.ExecuteReader();

                        if (reader.Read()) // If a record is found
                        {
                            // Create the session variables for successful login
                            Session["AdminId"] = Convert.ToInt32(reader["admin_id"]);
                            Session["AdminName"] = reader["admin_name"].ToString();
                            Session["AdminEmail"] = reader["admin_email"].ToString();

                            System.Diagnostics.Debug.WriteLine("Login Successful!");

                            // Redirect to the admin dashboard or home page
                            return RedirectToAction("Dashboard", "AdminMain");
                        }
                        else
                        {
                            // If no matching admin found
                            ModelState.AddModelError("", "Invalid email or password.");
                        }

                        reader.Close();
                    }
                }
            }

            // If we got this far, something failed, redisplay the login form
            return View(model);
        }


        // GET: Logout
        [HttpGet]
        public ActionResult Logout()
        {
            // Log out the user
            FormsAuthentication.SignOut();

            // Redirect to the Home controller's Index action
            return RedirectToAction("Index", "Home");
        }

        // GET: Register
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        // POST: Register
        
        [HttpPost]
        public ActionResult Register(AdminRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the email already exists
                var existingAdmin = db.Admin.FirstOrDefault(a => a.admin_email == model.Email);
                if (existingAdmin != null)
                {
                    ModelState.AddModelError("", "User with this email already exists.");
                    return View(model);
                }

                // Create new Admin entity with plain text password (Not recommended for production)
                var admin = new Admin
                {
                    admin_name = model.Username,
                    admin_email = model.Email,
                    admin_password = model.Password // Storing plain text password
                };

                // Save to the database
                db.Admin.Add(admin);
                db.SaveChanges();

                // Redirect to the login page after successful registration
                return RedirectToAction("Login", "AdminAccount");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // Hash password using SHA256
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

        protected override void Dispose(bool disposing)
        {
            //_context.Dispose();
            base.Dispose(disposing);
        }
    }
}