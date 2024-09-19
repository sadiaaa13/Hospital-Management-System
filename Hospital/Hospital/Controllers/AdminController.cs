using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Hospital.Models;

public class AdminController : Controller
{
    private HospitalEntities db = new HospitalEntities();

    // Dashboard View
    public ActionResult Dashboard()
    {
        return View("Dashboard", "_DoctorLayout");
    }

    // List Patients
    public ActionResult Patients()
    {
        // Fetch patients from database
        List<Patient> patients = GetPatientsFromDatabase();
        return View("Patients", "_DoctorLayout", patients);
    }

    // Upload Report
    [HttpPost]
    public ActionResult UploadReport(HttpPostedFileBase reportFile, int patientId)
    {
        try
        {
            if (reportFile != null && reportFile.ContentLength > 0)
            {
                var fileName = Path.GetFileName(reportFile.FileName);
                var uploadPath = Server.MapPath("~/UploadedReports");

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var path = Path.Combine(uploadPath, fileName);
                reportFile.SaveAs(path);

                // Save report to the database
                Report report = new Report
                {
                    p_id = patientId,
                    FileName = fileName,
                    FilePath = path,
                    DateTime_UploadDate = DateTime.Now
                };

                db.Reports.Add(report);
                db.SaveChanges();

                TempData["Message"] = "Report uploaded successfully!";
            }
            else
            {
                TempData["Error"] = "Please select a PDF file to upload.";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = "An error occurred while uploading the report.";
        }

        return RedirectToAction("Report");
    }

    // Fetch Reports
    public ActionResult Report()
    {
        var reports = db.Reports.ToList();
        return View("Report", "_DoctorLayout", reports);
    }

    // List Appointments
    public ActionResult Appointment()
    {
        List<Appointment> appointments = new List<Appointment>();

        string mainconn = ConfigurationManager.ConnectionStrings["HospitalEntities"].ConnectionString;
        SqlConnection sqlconn = new SqlConnection(mainconn);
        string sqlquery = "SELECT a_id, p_id, d_id, date, status, a_fname, a_lname, a_gender, a_email, a_contact FROM [dbo].[Appointment]";
        SqlCommand sqlcomm = new SqlCommand(sqlquery, sqlconn);
        sqlconn.Open();

        SqlDataReader reader = sqlcomm.ExecuteReader();
        while (reader.Read())
        {
            Appointment appointment = new Appointment
            {
                a_id = Convert.ToInt32(reader["a_id"]),
                p_id = Convert.ToInt32(reader["p_id"]),
                d_id = Convert.ToInt32(reader["d_id"]),
                date = Convert.ToDateTime(reader["date"]),
                status = reader["status"].ToString(),
                a_fname = reader["a_fname"].ToString(),
                a_lname = reader["a_lname"].ToString(),
                a_gender = reader["a_gender"].ToString(),
                a_email = reader["a_email"].ToString(),
                a_contact = Convert.ToInt32(reader["a_contact"])
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
        UpdateAppointmentStatus(id, "Confirmed");
        return RedirectToAction("Appointment");
    }

    // Cancel Appointment
    public ActionResult CancelAppointment(int id)
    {
        UpdateAppointmentStatus(id, "Cancelled");
        return RedirectToAction("Appointment");
    }

    // Helper method to update appointment status
    private void UpdateAppointmentStatus(int id, string status)
    {
        string mainconn = ConfigurationManager.ConnectionStrings["HospitalEntities"].ConnectionString;
        SqlConnection sqlconn = new SqlConnection(mainconn);
        string sqlquery = "UPDATE [dbo].[Appointment] SET status = @status WHERE a_id = @id";
        SqlCommand sqlcomm = new SqlCommand(sqlquery, sqlconn);
        sqlcomm.Parameters.AddWithValue("@status", status);
        sqlcomm.Parameters.AddWithValue("@id", id);
        sqlconn.Open();
        sqlcomm.ExecuteNonQuery();
        sqlconn.Close();
    }

    // Fetch Patients from Database
    private List<Patient> GetPatientsFromDatabase()
    {
        List<Patient> patients = new List<Patient>();

        string mainconn = ConfigurationManager.ConnectionStrings["HospitalEntities"].ConnectionString;
        SqlConnection sqlconn = new SqlConnection(mainconn);
        string sqlquery = "SELECT p_id, p_firstname, p_lastname, p_email FROM [dbo].[Patient]";
        SqlCommand sqlcomm = new SqlCommand(sqlquery, sqlconn);
        sqlconn.Open();

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
