using Hospital.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.IO;
namespace Hospital.Controllers
{
    public class PatientController : Controller
    {
        // GET: Patient
        public ActionResult Patient_Dashboard()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult login()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        [HttpPost]
        public ActionResult login(string email, string password)
        {
            string mainconn = ConfigurationManager.ConnectionStrings["HospitalEntities"].ConnectionString;
            using (SqlConnection sqlconn = new SqlConnection(mainconn))
            {
                string query = "SELECT TOP 1 * FROM Patient WHERE p_email = @Email AND p_password = @Password";
                SqlCommand cmd = new SqlCommand(query, sqlconn);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Password", password);

                sqlconn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    var patient = new
                    {
                        
                        p_name = reader["p_firstname"].ToString(),
                        p_id = reader["p_id"].ToString(),
                    };

                    Session["name"] = patient.p_name;
                    Session["Id"] = patient.p_id;

                    return RedirectToAction("Patient_Dashboard", "Patient");
                }
                else
                {
                    ModelState.AddModelError("Invalid email or password", "Invalid email or password.");
                    return View();
                }

            }

        }

        public ActionResult registration()
        {


            return View();
        }

        [HttpPost]
        public ActionResult registration(Patient_reg reg, HttpPostedFileBase file)
        {

            string mainconn = ConfigurationManager.ConnectionStrings["HospitalEntities"].ConnectionString;
            SqlConnection sqlconn = new SqlConnection(mainconn);
            string sqlquery = "insert into [dbo].[Patient](p_firstname,p_lastname,p_email,p_password) values(@p_firstname,@p_lastname,@p_email,@p_password)";
            SqlCommand sqlcomm = new SqlCommand(sqlquery, sqlconn);
            sqlconn.Open();
            sqlcomm.Parameters.AddWithValue("@p_firstname", reg.p_firstname);
            sqlcomm.Parameters.AddWithValue("@p_lastname", reg.p_lastname);
            sqlcomm.Parameters.AddWithValue("@p_email", reg.p_email);
            sqlcomm.Parameters.AddWithValue("@p_password", reg.p_password);

            sqlcomm.ExecuteNonQuery();
            sqlconn.Close();



            return View();
        }
        public ActionResult Find_a_doctor()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult Request_for_an_apointment()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult not_available()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
       public ActionResult patient_profile()
        {

            ViewBag.Message = "Your contact page.";
            return View();

        }
        private Patient GetLoggedInPatientProfile(string loggedInPatientEmail)
        {
            Patient patient = null;

            string mainconn = ConfigurationManager.ConnectionStrings["HospitalEntities"].ConnectionString;
            SqlConnection sqlconn = new SqlConnection(mainconn);
            string sqlquery = "SELECT p_id, p_firstname, p_lastname, p_email FROM [dbo].[Patient] WHERE p_email = @p_email";
            SqlCommand sqlcomm = new SqlCommand(sqlquery, sqlconn);

            // Add parameter to prevent SQL injection
            sqlcomm.Parameters.AddWithValue("@p_email", loggedInPatientEmail);

            sqlconn.Open();
            SqlDataReader reader = sqlcomm.ExecuteReader();

            if (reader.Read())
            {
                patient = new Patient
                {
                    p_id = Convert.ToInt32(reader["p_id"]),
                    p_firstname = reader["p_firstname"].ToString(),
                    p_lastname = reader["p_lastname"].ToString(),
                    p_email = reader["p_email"].ToString()
                };
            }

            reader.Close();
            sqlconn.Close();

            return patient;
        }
    }
   
   
}