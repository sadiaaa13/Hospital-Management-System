using Hospital.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.Mvc;

namespace Hospital.Controllers
{
    public class PatientController : Controller
    {
        // GET: Patient

        public ActionResult Patient_Dashboard()
        {
            // Proceed if logged in
            return View("Patient_Dashboard", "_patientLayout");

        }

        public ActionResult login()
        {
            return View("login", "_patientLayout");
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
                        p_email = reader["p_email"].ToString(),  // Store the email in session for profile
                        p_id = reader["p_id"].ToString(),
                    };

                    Session["p_name"] = patient.p_name;
                    Session["p_email"] = patient.p_email;  // Store email in session
                    Session["p_id"] = patient.p_id;

                    return RedirectToAction("Patient_Dashboard", "Patient");
                }
                else
                {
                    ModelState.AddModelError("Invalid email or password", "Invalid email or password.");
                    return View("login", "_patientLayout");
                }
            }
        }

        public ActionResult registration()
        {
            return View("registration", "_patientLayout");
        }

        [HttpPost]
        public ActionResult registration(Patient_reg reg, HttpPostedFileBase file)
        {
            string mainconn = ConfigurationManager.ConnectionStrings["HospitalEntities"].ConnectionString;
            using (SqlConnection sqlconn = new SqlConnection(mainconn))
            {
                string sqlquery = "INSERT INTO [dbo].[Patient](p_firstname, p_lastname, p_email, p_password) VALUES(@p_firstname, @p_lastname, @p_email, @p_password)";
                SqlCommand sqlcomm = new SqlCommand(sqlquery, sqlconn);
                sqlcomm.Parameters.AddWithValue("@p_firstname", reg.p_firstname);
                sqlcomm.Parameters.AddWithValue("@p_lastname", reg.p_lastname);
                sqlcomm.Parameters.AddWithValue("@p_email", reg.p_email);
                sqlcomm.Parameters.AddWithValue("@p_password", reg.p_password);

                sqlconn.Open();
                sqlcomm.ExecuteNonQuery();
            }

            return View("login", "_patientLayout");  // Redirect to login after successful registration
        }

        public ActionResult Find_a_doctor()
        {
            List<FDoctor> doctors = new List<FDoctor>();

            string mainconn = ConfigurationManager.ConnectionStrings["HospitalEntities"].ConnectionString;
            using (SqlConnection sqlconn = new SqlConnection(mainconn))
            {
                string query = "SELECT df_id, df_image, df_name, df_spe FROM [dbo].[FDoctor]";
                SqlCommand cmd = new SqlCommand(query, sqlconn);

                sqlconn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    FDoctor doctor = new FDoctor
                    {
                        df_id = Convert.ToInt32(reader["df_id"]),
                        df_image = reader["df_image"].ToString(),
                        df_name = reader["df_name"].ToString(),
                        df_spe = reader["df_spe"].ToString()
                    };

                    doctors.Add(doctor);
                }

                reader.Close();
            }

            return View("Find_a_doctor", doctors);
        }

        public ActionResult Request_for_an_apointment()
        {
            return View("Request_for_an_apointment", "_patientLayout");
        }

        public ActionResult not_available()
        {
            return View("not_available", "_patientLayout");
        }

        // Patient Profile Method
        public ActionResult patient_profile()
        {
            // Get the logged-in patient's email from the session
            string loggedInPatientEmail = Session["p_email"]?.ToString();

            if (!string.IsNullOrEmpty(loggedInPatientEmail))
            {
                // Fetch the patient profile from the database using the email stored in session
                Patient patient = GetLoggedInPatientProfile(loggedInPatientEmail);

                if (patient != null)
                {
                    // Pass patient details to the view using ViewBag
                    ViewBag.PatientName = $"{patient.p_firstname} {patient.p_lastname}";
                    ViewBag.PatientEmail = patient.p_email;
                    ViewBag.PatientID = patient.p_id;
                }
                else
                {
                    ViewBag.PatientName = "Unknown";
                    ViewBag.PatientEmail = "Unknown";
                }
            }
            else
            {
                ViewBag.PatientName = "Unknown";
                ViewBag.PatientEmail = "Unknown";
            }

            return View("patient_profile", "_patientLayout");
        }

        private Patient GetLoggedInPatientProfile(string loggedInPatientEmail)
        {
            Patient patient = null;

            string mainconn = ConfigurationManager.ConnectionStrings["HospitalEntities"].ConnectionString;

            using (SqlConnection sqlconn = new SqlConnection(mainconn))
            {
                string sqlquery = "SELECT p_id, p_firstname, p_lastname, p_email FROM [dbo].[Patient] WHERE p_email = @p_email";
                SqlCommand sqlcomm = new SqlCommand(sqlquery, sqlconn);
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
            }

            return patient;
        }
        public ActionResult LogOut()
        {
            // Clear all session data
            Session.Clear();

            // Redirect to the login page
            return RedirectToAction("login", "Patient");
        }

    }
}
