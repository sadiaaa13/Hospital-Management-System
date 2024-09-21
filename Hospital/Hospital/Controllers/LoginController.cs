using Hospital.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Permissions;
using System.Xml.Linq;



namespace Hospital.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Login()
        {
            return View("Login", "_loginLayout");
        }
        [HttpPost]
        public ActionResult Login(Doctor doc)
        {
            using (HospitalEntities mm = new HospitalEntities())
            {

            }
            return View("Login", "_loginLayout");
        }
        public ActionResult LoginStaff()
        {
            return View("LoginStaff", "_loginLayout");
        }

        public ActionResult LoginDoctor()
        {
            return View("LoginDoctor", "_loginLayout");
        }
        [HttpPost]
        public ActionResult LoginDoctor(FDoctor f)
        {
            string check = "";
            string email = f.df_email;
            string mainconn = ConfigurationManager.ConnectionStrings["HospitalEntities"].ConnectionString;
            SqlConnection sqlconn = new SqlConnection(mainconn);
            string sqlquery = "SELECT df_password FROM [dbo].[FDoctor] Where df_email =@email";
            SqlCommand sqlcomm = new SqlCommand(sqlquery, sqlconn);
            sqlcomm.Parameters.AddWithValue("@email", email);
            sqlconn.Open();
            SqlDataReader reader = sqlcomm.ExecuteReader();

            if (reader.Read())
            {
                check = reader["df_password"].ToString();

            }
            if (check.Trim() == f.df_password.Trim())
            {
                ViewBag.che = check;
                return RedirectToAction("StaffDash", "Staff");
            }

            reader.Close();
            sqlconn.Close();
            
            return View();
        }
        public ActionResult RegistrationDoctor()
        {
            return View("RegistrationDoctor", "_loginLayout");
        }
        [HttpPost]
        public ActionResult RegistrationDoctor(FDoctor temp)
        {
            string filename = Path.GetFileNameWithoutExtension(temp.DfImage.FileName);
            string extension = Path.GetExtension(temp.DfImage.FileName);
            filename = filename + DateTime.Now.ToString("yymmssfff") + extension;
            temp.df_image = "~/Image/" + filename;
            filename = Path.Combine(Server.MapPath("~/Image/"), filename);
            temp.DfImage.SaveAs(filename);


            string filename1 = Path.GetFileNameWithoutExtension(temp.DfPdf.FileName);
            string extension1 = Path.GetExtension(temp.DfPdf.FileName);
            filename1 = filename1 + DateTime.Now.ToString("yymmssfff") + extension1;
            temp.df_resume = "~/PdfFile/" + filename1;
            filename1 = Path.Combine(Server.MapPath("~/PdfFile/"), filename1);
            temp.DfPdf.SaveAs(filename1);


            string mainconn = ConfigurationManager.ConnectionStrings["HospitalEntities"].ConnectionString;
            SqlConnection sqlconn = new SqlConnection(mainconn);
            string sqlquery = "insert into [dbo].[FDoctor](df_name,df_email,df_phone,df_address,df_spe,df_image,df_resume,df_experience,df_password) values(@df_name,@df_email,@df_phone,@df_address,@df_spe,@df_image,@df_resume,@df_experience,@df_password)";
            SqlCommand sqlcomm = new SqlCommand(sqlquery, sqlconn);
            sqlconn.Open();
            sqlcomm.Parameters.AddWithValue("@df_name", temp.df_name);
            sqlcomm.Parameters.AddWithValue("@df_email", temp.df_email);
            sqlcomm.Parameters.AddWithValue("@df_phone", temp.df_phone);
            sqlcomm.Parameters.AddWithValue("@df_address", temp.df_address);
            sqlcomm.Parameters.AddWithValue("@df_spe", temp.df_spe);
            sqlcomm.Parameters.AddWithValue("@df_image", temp.df_image);
            sqlcomm.Parameters.AddWithValue("@df_resume", temp.df_resume);
            sqlcomm.Parameters.AddWithValue("@df_experience", temp.df_experience);
            sqlcomm.Parameters.AddWithValue("@df_password", temp.df_password);
            sqlcomm.ExecuteNonQuery();
            sqlconn.Close();
            return View("RegistrationDoctor", "_loginLayout");
        }

        public ActionResult ShowTempDoctor()
        {
            List<DoctorTemp> doctorTemps = new List<DoctorTemp>();

            string mainconn = ConfigurationManager.ConnectionStrings["HospitalEntities"].ConnectionString;
            SqlConnection sqlconn = new SqlConnection(mainconn);
            string sqlquery = "SELECT * FROM [dbo].[DoctorTemp]";
            SqlCommand sqlcomm = new SqlCommand(sqlquery, sqlconn);
            sqlconn.Open();

            SqlDataReader reader = sqlcomm.ExecuteReader();
            while (reader.Read())
            {
                DoctorTemp doctor = new DoctorTemp
                {
                    dt_id = Convert.ToInt32(reader["dt_id"]),
                    dt_fname = reader["dt_fname"].ToString(),
                    dt_email = reader["dt_email"].ToString(),
                    dt_phone = Convert.ToInt32(reader["dt_phone"]),
                    dt_address = reader["dt_address"].ToString(),
                    dt_spe = reader["dt_spe"].ToString(),
                    dt_image = reader["dt_image"].ToString(),
                    dt_resume = reader["dt_resume"].ToString(),
                    dt_experience = reader["dt_experience"].ToString()

                };
                doctorTemps.Add(doctor);
            }
            reader.Close();
            sqlconn.Close();

            return View(doctorTemps);
            //return View();
        }
        public ActionResult DoctorLogin()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DoctorLogin(FDoctor f)
        {
            
            return View();
        }

    }
}