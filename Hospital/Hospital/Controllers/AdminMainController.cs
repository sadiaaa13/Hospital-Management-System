using Demo.Models;
using Demo.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Demo.Controllers
{
    public class AdminMainController : Controller
    {
        private NewHospitalEntities db = new NewHospitalEntities();
        // GET: AdminMain
        public ActionResult Dashboard()
        {
            return View();
        }
        public ActionResult Patients()
        {
            // Assuming you have a database context named 'db'
            var patientList = (from p in db.Patient
                               join a in db.Appointment on p.p_id equals a.p_id into pa
                               from a in pa.DefaultIfEmpty()
                               join d in db.Doctor on a.d_id equals d.d_id into ad
                               from d in ad.DefaultIfEmpty()
                               join r in db.Room on p.p_id equals r.p_id into pr
                               from r in pr.DefaultIfEmpty()
                               select new PatientViewModel
                               {
                                   p_id = p.p_id,
                                   p_name = p.p_firstname + " " + p.p_lastname,
                                   checkin_date = a.date,
                                   doctor_name = d != null ? d.d_name : string.Empty,
                                   disease = "",  // Assuming disease is stored somewhere else
                                   room_number = r != null ? r.room_number.ToString() : "Not Assigned"
                               }).ToList();

            // Pass data to the view
            ViewBag.PatientList = patientList;
            return View();
        }

        [HttpGet]
        public ActionResult DeletePatient(int id)
        {
            var patient = db.Patient.Find(id);
            if (patient != null)
            {
                // Delete related data if needed (e.g., Appointments, Rooms, etc.)
                db.Patient.Remove(patient);
                db.SaveChanges();
            }

            return RedirectToAction("Patients");
        }

        [HttpPost]
   
        public ActionResult SavePatient(int p_id, string p_name, string disease, DateTime checkin_date, string room_number)
        {
            // Find the patient in the database
            var patient = db.Patient.Find(p_id);
            if (patient != null)
            {
                // Update patient information
                patient.p_firstname = p_name.Split(' ')[0];
                patient.p_lastname = p_name.Split(' ').Length > 1 ? p_name.Split(' ')[1] : "";

                // Assuming you store disease information in the Patient entity for now:
                // patient.disease = disease;

                // Find the related appointment and update check-in date
                var appointment = db.Appointment.FirstOrDefault(a => a.p_id == p_id);
                if (appointment != null)
                {
                    appointment.date = checkin_date;
                }

                // Update or assign room if the room number is provided
                var room = db.Room.FirstOrDefault(r => r.room_number == room_number);
                if (room != null)
                {
                    room.p_id = p_id;  // Assign the patient to the room
                }

                // Save changes to the database
                db.SaveChanges();
            }

            return RedirectToAction("Patients");
        }



        // GET: Doctorlist
        public ActionResult Doctorlist()
        {
            // Fetch all doctors from the database to display them
            var doctorList = db.Doctor.ToList();
            ViewBag.DoctorList = doctorList;

            return View();
        }

        // POST: AddDoctor
        [HttpPost]
        public ActionResult AddDoctor(Doctor model)
        {
            if (ModelState.IsValid)
            {
                var newDoctor = new Doctor
                {
                    d_name = model.d_name,
                    d_dept = model.d_dept,
                    d_contact = model.d_contact,
                    d_wh = model.d_wh
                };

                db.Doctor.Add(newDoctor);
                db.SaveChanges();

                return RedirectToAction("Doctorlist");
            }

            return View(model);
        }

        // GET: DeleteDoctor
        public ActionResult DeleteDoctor(int id)
        {
            var doctor = db.Doctor.Find(id);
            if (doctor != null)
            {
                db.Doctor.Remove(doctor);
                db.SaveChanges();
            }

            return RedirectToAction("Doctorlist");
        }

        [HttpGet]
        public ActionResult Medicine()
        {
            // Fetch all medicines from the database to display them
            var medicineList = db.Medicine.ToList();
            ViewBag.MedicineList = medicineList;

            return View();
        }

        // POST: AddMedicine
        [HttpPost]
        public ActionResult Medicine(Medicine model)
        {
            if (ModelState.IsValid)
            {
                // Check if a medicine with the same name already exists
                var existingMedicine = db.Medicine.FirstOrDefault(m => m.med_name == model.med_name);
                if (existingMedicine != null)
                {
                    // If it exists, update the quantity
                    existingMedicine.med_quantity += model.med_quantity;
                }
                else
                {
                    // If it doesn't exist, create a new medicine
                    var newMedicine = new Medicine
                    {
                        med_name = model.med_name,
                        med_description = model.med_description,
                        med_price = model.med_price,
                        med_quantity = model.med_quantity
                    };
                    db.Medicine.Add(newMedicine);
                }

                db.SaveChanges();

                // After saving, fetch the updated list of medicines
                var medicineList = db.Medicine.ToList();
                ViewBag.MedicineList = medicineList;

                return RedirectToAction("Medicine");
            }

            return View(model);
        }

        // GET: Delete Medicine
        public ActionResult DeleteMedicine(int id)
        {
            var medicine = db.Medicine.Find(id);
            if (medicine != null)
            {
                db.Medicine.Remove(medicine);
                db.SaveChanges();
            }

            return RedirectToAction("Medicine");
        }

        /*public ActionResult Rooms()
        {
            return View();
        }
        */
        // GET: Room List
        public ActionResult Rooms()
        {
            // Fetch all rooms without patient data
            var rooms = db.Room.ToList();

            // Pass the rooms data to the view via ViewBag or ViewModel
            ViewBag.AssignedRooms = rooms;

            return View();
        }


        // Assign Room Action
        [HttpPost]
        
        public ActionResult AssignRoom(string patientName, string roomNumber, decimal price, int capacity)
        {
            var room = new Room
            {
                room_number = roomNumber,
                room_price = price,
                capacity = capacity,
                PatientName = patientName // Directly store patient name
            };

            db.Room.Add(room);
            db.SaveChanges();

            return RedirectToAction("Rooms");
        }

        // Remove Room Assignment Action
        [HttpPost]
        public ActionResult RemoveRoomAssignment(int roomId)
        {
            var room = db.Room.Find(roomId);
            if (room != null)
            {
                db.Room.Remove(room);
                db.SaveChanges();
            }
            return RedirectToAction("Rooms");
        }

        // Edit Room Assignment Action
        [HttpPost]
        public ActionResult EditRoomAssignment(int roomId, string editPatientName, string editRoomNumber, decimal editPrice, int editCapacity)
        {
            var room = db.Room.Find(roomId);
            var patient = db.Patient.FirstOrDefault(p => p.p_firstname + " " + p.p_lastname == editPatientName);

            if (room != null)
            {
                room.room_number = editRoomNumber;
                room.room_price = editPrice;
                room.capacity = editCapacity;

                if (patient != null)
                {
                    room.p_id = patient.p_id;
                }
                db.SaveChanges();
            }
            return RedirectToAction("Rooms");
        }
        // GET: Reviews
        public ActionResult Reviews()
        {
            var reviews = db.Review.ToList(); // Fetch all reviews
            return View(reviews); // Pass reviews to the view
        }

        // POST: DeleteReview
        [HttpPost]
        public ActionResult DeleteReview(int id)
        {
            var review = db.Review.Find(id);
            if (review != null)
            {
                db.Review.Remove(review);
                db.SaveChanges();
            }
            return RedirectToAction("Reviews"); // Redirect to the reviews list
        }

        // POST: PostReview
        [HttpPost]
        public ActionResult PostReview(int id)
        {
            var review = db.Review.Find(id);
            if (review != null)
            {
                // Logic for posting review (e.g., marking it as posted or moving to another list)
                // You can add another column to indicate if it's posted, or redirect to index, etc.

                TempData["PostedReview"] = review.review_text; // Store review text to show on the index page
            }
            return RedirectToAction("Index", "Home"); // Redirect to the Home page
        }

        [HttpGet]
        public ActionResult Notice()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Notice(Notice model)
        {
            if (ModelState.IsValid)
            {
                model.post_date = DateTime.Now;
                db.Notice.Add(model);
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }
    }
}