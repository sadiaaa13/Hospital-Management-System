using Hospital.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Hospital.Controllers
{
    public class AdminController : Controller
    {
        private HospitalEntities db = new HospitalEntities();

        // Dashboard View
        public ActionResult Dashboard()
        {
            string loggedInEmail = Session["LoggedUserEmail"] as string;

            // Use the email in your logic
            ViewBag.Email = loggedInEmail;

            return View("Dashboard", "_DoctorLayout");
        }

        // List Patients
        public ActionResult Patients()
        {
            List<Patient> patients = GetPatientsFromDatabase();
            return View("Patients", "_DoctorLayout", patients);
        }

        // List Appointments for specific doctor
        public ActionResult Appointment()
        {
            List<Appointment> appointments = new List<Appointment>();

            string loggedInEmail = Session["LoggedUserEmail"] as string;

            // Fetch doctor ID using email
            string mainconn = ConfigurationManager.ConnectionStrings["HospitalEntities"].ConnectionString;
            SqlConnection sqlconn = new SqlConnection(mainconn);
            sqlconn.Open();
            string sqlQuery = "SELECT df_id FROM [dbo].[FDoctor] WHERE df_email = @df_email";
            SqlCommand sqlcomm = new SqlCommand(sqlQuery, sqlconn);
            sqlcomm.Parameters.AddWithValue("@df_email", loggedInEmail);
            int dfId = (int)sqlcomm.ExecuteScalar();
            sqlconn.Close();

            // Fetch appointments for the specific doctor
            sqlconn.Open();
            sqlQuery = "SELECT a_id, p_id, df_id, date, status, a_gender, a_email, a_contact FROM [dbo].[Appointment] WHERE df_id = @df_id";
            sqlcomm = new SqlCommand(sqlQuery, sqlconn);
            sqlcomm.Parameters.AddWithValue("@df_id", dfId);
            SqlDataReader reader = sqlcomm.ExecuteReader();

            while (reader.Read())
            {
                Appointment appointment = new Appointment
                {
                    a_id = Convert.ToInt32(reader["a_id"]),
                    p_id = Convert.ToInt32(reader["p_id"]),
                    df_id = Convert.ToInt32(reader["df_id"]),
                    date = Convert.ToDateTime(reader["date"]),
                    status = reader["status"].ToString(),
                    a_gender = reader["a_gender"].ToString(),
                    a_email = reader["a_email"].ToString(),
                    a_contact = reader["a_contact"].ToString()
                };
                appointments.Add(appointment);
            }
            reader.Close();
            sqlconn.Close();

            return View(appointments);
        }

        // Confirm Appointment
        public ActionResult ConfirmAppointment(int id)
        {
            try
            {
                UpdateAppointmentStatus(id, "Confirmed");
                TempData["Message"] = "Appointment confirmed successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error confirming appointment: " + ex.Message;
            }
            return RedirectToAction("Appointment");
        }

        // Cancel Appointment
        public ActionResult CancelAppointment(int id)
        {
            try
            {
                UpdateAppointmentStatus(id, "Cancelled");
                TempData["Message"] = "Appointment cancelled successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error cancelling appointment: " + ex.Message;
            }
            return RedirectToAction("Appointment");
        }

        // Complete Appointment
        public ActionResult CompleteAppointment(int id)
        {
            try
            {
                UpdateAppointmentStatus(id, "Completed");
                TempData["Message"] = "Appointment marked as completed.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error marking appointment as completed: " + ex.Message;
            }
            return RedirectToAction("Appointment");
        }

        // Helper method to update appointment status
        private void UpdateAppointmentStatus(int id, string status)
        {
            using (var sqlconn = new SqlConnection(ConfigurationManager.ConnectionStrings["HospitalEntities"].ConnectionString))
            {
                sqlconn.Open();
                string sqlQuery = "UPDATE [dbo].[Appointment] SET status = @status WHERE a_id = @id";
                using (var sqlcomm = new SqlCommand(sqlQuery, sqlconn))
                {
                    sqlcomm.Parameters.AddWithValue("@status", status);
                    sqlcomm.Parameters.AddWithValue("@id", id);
                    sqlcomm.ExecuteNonQuery();
                }
            }
        }

        // Doctor Profile
        public ActionResult DoctorProfile()
        {
            string loggedInEmail = Session["LoggedUserEmail"] as string;
            FDoctor doctor = GetDoctorProfile(loggedInEmail);
            return View("DoctorProfile", "_DoctorLayout", doctor);
        }

        // Edit Doctor Profile
        [HttpPost]
        public ActionResult EditDoctorProfile(FDoctor updatedDoctor, HttpPostedFileBase newImage)
        {
            try
            {
                string loggedInEmail = Session["LoggedUserEmail"] as string;

                // Get the existing doctor's record
                FDoctor existingDoctor = null;

                string mainconn = ConfigurationManager.ConnectionStrings["HospitalEntities"].ConnectionString;
                using (SqlConnection sqlconn = new SqlConnection(mainconn))
                {
                    sqlconn.Open();

                    // Fetch the existing doctor record by email
                    string fetchQuery = @"SELECT df_image FROM [dbo].[FDoctor] WHERE df_email = @df_email";
                    using (SqlCommand fetchComm = new SqlCommand(fetchQuery, sqlconn))
                    {
                        fetchComm.Parameters.AddWithValue("@df_email", loggedInEmail);

                        using (SqlDataReader reader = fetchComm.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Assuming df_image is at index 0
                                existingDoctor = new FDoctor
                                {
                                    df_image = reader["df_image"] as string
                                };
                            }
                            else
                            {
                                TempData["Error"] = "Doctor not found!";
                                return RedirectToAction("DoctorProfile");
                            }
                        }
                    }

                    // Handle the new profile image upload
                    if (newImage != null && newImage.ContentLength > 0)
                    {
                        // Ensure valid extensions (optional)
                        string[] validExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
                        string extension = Path.GetExtension(newImage.FileName);
                        if (!validExtensions.Contains(extension.ToLower()))
                        {
                            TempData["Error"] = "Invalid file format. Only .jpg, .jpeg, .png, and .gif are allowed.";
                            return RedirectToAction("EditDoctorProfile");
                        }

                        string fileName = Path.GetFileNameWithoutExtension(newImage.FileName);
                        fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;

                        // Save only the relative path to the image in the database
                        string imagePath = "/Images/" + fileName; // This will be stored in the database as a relative path
                        string fullPath = Path.Combine(Server.MapPath("~/Images/"), fileName); // This is the physical path to save the image file

                        // Save the new image file
                        newImage.SaveAs(fullPath);

                        // Update the image path in the doctor object
                        updatedDoctor.df_image = imagePath;
                    }
                    else
                    {
                        // If no new image uploaded, keep the existing image
                        updatedDoctor.df_image = existingDoctor.df_image;
                    }

                    // Update doctor details in the database
                    string sqlQuery = @"UPDATE [dbo].[FDoctor] 
                                SET df_name = @df_name, 
                                    df_phone = @df_phone, 
                                    df_address = @df_address, 
                                    df_spe = @df_spe, 
                                    df_experience = @df_experience,
                                    df_image = @df_image -- Update the image path
                                WHERE df_email = @df_email";

                    using (SqlCommand sqlcomm = new SqlCommand(sqlQuery, sqlconn))
                    {
                        sqlcomm.Parameters.AddWithValue("@df_name", updatedDoctor.df_name);
                        sqlcomm.Parameters.AddWithValue("@df_phone", updatedDoctor.df_phone);
                        sqlcomm.Parameters.AddWithValue("@df_address", updatedDoctor.df_address);
                        sqlcomm.Parameters.AddWithValue("@df_spe", updatedDoctor.df_spe);
                        sqlcomm.Parameters.AddWithValue("@df_experience", updatedDoctor.df_experience);
                        sqlcomm.Parameters.AddWithValue("@df_image", updatedDoctor.df_image ?? (object)DBNull.Value); // Handle null image
                        sqlcomm.Parameters.AddWithValue("@df_email", loggedInEmail);

                        sqlcomm.ExecuteNonQuery();
                    }
                }

                TempData["Message"] = "Profile updated successfully!";
                return RedirectToAction("DoctorProfile");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error updating profile: " + ex.Message;
                return RedirectToAction("EditDoctorProfile");
            }
        }



        // Fetch Doctor Profile from Database
        private FDoctor GetDoctorProfile(string email)
        {
            FDoctor doctor = null;

            string mainconn = ConfigurationManager.ConnectionStrings["HospitalEntities"].ConnectionString;
            using (SqlConnection sqlconn = new SqlConnection(mainconn))
            {
                sqlconn.Open();
                string sqlQuery = "SELECT * FROM [dbo].[FDoctor] WHERE df_email = @df_email";
                using (SqlCommand sqlcomm = new SqlCommand(sqlQuery, sqlconn))
                {
                    sqlcomm.Parameters.AddWithValue("@df_email", email);
                    SqlDataReader reader = sqlcomm.ExecuteReader();

                    if (reader.Read())
                    {
                        doctor = new FDoctor
                        {
                            df_id = Convert.ToInt32(reader["df_id"]),
                            df_name = reader["df_name"].ToString(),
                            df_email = reader["df_email"].ToString(),
                            df_phone = reader["df_phone"].ToString(),
                            df_address = reader["df_address"].ToString(),
                            df_spe = reader["df_spe"].ToString(),
                            df_image = reader["df_image"].ToString(),
                            df_resume = reader["df_resume"].ToString(),
                        };
                    }
                }
            }

            return doctor;
        }
        public ActionResult Report()
        {
            List<Report> reports = GetReportsFromDatabase();
            return View(reports); // Pass reports to the view
        }

        // Fetch Reports from Database
        private List<Report> GetReportsFromDatabase()
        {
            List<Report> reports = new List<Report>();

            string mainconn = ConfigurationManager.ConnectionStrings["HospitalEntities"].ConnectionString;
            using (SqlConnection sqlconn = new SqlConnection(mainconn))
            {
                sqlconn.Open();
                string sqlQuery = "SELECT * FROM [dbo].[Report]";
                using (SqlCommand sqlcomm = new SqlCommand(sqlQuery, sqlconn))
                {
                    SqlDataReader reader = sqlcomm.ExecuteReader();

                    while (reader.Read())
                    {
                        Report report = new Report
                        {
                            ReportId = Convert.ToInt32(reader["ReportId"]),
                            p_id = Convert.ToInt32(reader["p_id"]),
                            FileName = reader["FileName"].ToString(),
                            FilePath = reader["FilePath"].ToString(),
                           // DateTime_UploadDate = Convert.ToDateTime(reader["DateTime_UploadDate"]),
                            df_id = reader["df_id"] as int? // Nullable<int>
                        };
                        reports.Add(report);
                    }
                }
            }

            return (reports);
        }


        // Fetch Patients from Database
        private List<Patient> GetPatientsFromDatabase()
        {
            List<Patient> patients = new List<Patient>();

            string mainconn = ConfigurationManager.ConnectionStrings["HospitalEntities"].ConnectionString;
            SqlConnection sqlconn = new SqlConnection(mainconn);
            sqlconn.Open();
            string sqlQuery = "SELECT p_id, p_firstname, p_lastname, p_email FROM [dbo].[Patient]";
            SqlCommand sqlcomm = new SqlCommand(sqlQuery, sqlconn);
            SqlDataReader reader = sqlcomm.ExecuteReader();

            while (reader.Read())
            {
                Patient patient = new Patient
                {
                    p_id = Convert.ToInt32(reader["p_id"]),
                    p_firstname = reader["p_firstname"].ToString(),
                    p_lastname = reader["p_lastname"].ToString(),
                    p_email = reader["p_email"].ToString()
                };
                patients.Add(patient);
            }
            reader.Close();
            sqlconn.Close();

            return patients;
        }
    }
}