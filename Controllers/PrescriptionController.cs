using DocBook.Models;
using DocBook.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;


namespace DocBook.Controllers
{
    public class PrescriptionController : Controller
    {
        private readonly PrescriptionRepository _repo;
        private readonly AppointmentRepository _appRepo;
        private readonly string _connectionString = "Data Source=.;Initial Catalog=HospitalDB;Integrated Security=True;Trust Server Certificate=True;";
        private readonly IConfiguration _configuration;

        public PrescriptionController(PrescriptionRepository repo, AppointmentRepository appRepo, IConfiguration configuration)
        {
            _repo = repo;
            _appRepo = appRepo;
            _configuration = configuration;
        }

        public IActionResult Index(int appointmentId)
        {
            var prescriptions = new List<Prescription>();
            Appointment appointment = null;

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Get appointment with doctor & patient
                string appointmentQuery = @"
            SELECT a.AppointmentId, 
                   a.AppointmentDate,
                   p.Name AS PatientName,
                   d.Name AS DoctorName
            FROM Appointments a
            INNER JOIN Patients p ON a.PatientId = p.PatientId
            INNER JOIN Doctors d ON a.DoctorId = d.DoctorId
            WHERE a.AppointmentId = @AppointmentId";

                using (SqlCommand cmd = new SqlCommand(appointmentQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            appointment = new Appointment
                            {
                                AppointmentId = (int)reader["AppointmentId"],
                                AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"]),
                                PatientName = reader["PatientName"].ToString(),
                                DoctorName = reader["DoctorName"].ToString()
                            };
                        }
                    }
                }

                // Get prescriptions
                string prescriptionQuery = @"
            SELECT PrescriptionId, MedicineName, Dosage, Instructions, PrescribedOn
            FROM Prescriptions
            WHERE AppointmentId = @AppointmentId";

                using (SqlCommand cmd = new SqlCommand(prescriptionQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            prescriptions.Add(new Prescription
                            {
                                PrescriptionId = (int)reader["PrescriptionId"],
                                MedicineName = reader["MedicineName"].ToString(),
                                Instructions = reader["Instructions"].ToString(),
                                PrescribedOn = Convert.ToDateTime(reader["PrescribedOn"]),
                                Dosage = reader["Dosage"].ToString()
                            });
                        }
                    }
                }
            }

            ViewBag.Appointment = appointment;
            return View(prescriptions);
        }



        public IActionResult Create(int appointmentId)
        {
            Appointment ap = null;
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(@"
            SELECT a.AppointmentId,
                   a.PatientId,
                   p.Name AS PatientName,
                   a.DoctorId,
                   d.Name AS DoctorName,
                   a.AppointmentDate
            FROM Appointments a
            INNER JOIN Patients p ON a.PatientId = p.PatientId
            INNER JOIN Doctors d ON a.DoctorId = d.DoctorId
            WHERE a.AppointmentId = @AppointmentId", con);

                cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        ap = new Appointment
                        {
                            AppointmentId = Convert.ToInt32(dr["AppointmentId"]),
                            PatientId = Convert.ToInt32(dr["PatientId"]),
                            PatientName = dr["PatientName"].ToString(),
                            DoctorId = Convert.ToInt32(dr["DoctorId"]),
                            DoctorName = dr["DoctorName"].ToString(),
                            AppointmentDate = Convert.ToDateTime(dr["AppointmentDate"])
                        };
                    }
                }
            }

            if (ap == null)
            {
                return NotFound("Appointment not found.");
            }

            ViewBag.Appointment = ap;
            return View(new PrescriptionViewModel { AppointmentId = appointmentId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PrescriptionViewModel vm)
        {
           

            // ✅ Insert into Prescriptions table
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(@"
                INSERT INTO Prescriptions 
               (AppointmentId, MedicineName, Dosage, Instructions, PrescribedOn) 
               VALUES (@AppointmentId, @MedicineName, @Dosage, @Instructions, @PrescribedOn)", con);

                cmd.Parameters.AddWithValue("@AppointmentId", vm.AppointmentId);
                cmd.Parameters.AddWithValue("@MedicineName", vm.MedicineName);
                cmd.Parameters.AddWithValue("@Dosage", vm.Dosage);
                cmd.Parameters.AddWithValue("@Instructions", vm.Instructions ?? "");
                cmd.Parameters.AddWithValue("@PrescribedOn", DateTime.Now);

                cmd.ExecuteNonQuery();
            }

            return RedirectToAction(nameof(Index), new { appointmentId = vm.AppointmentId });
        }


        public IActionResult Edit(int id)
        {
            var p = _repo.GetById(id);
            if (p == null) return NotFound();
            var vm = new PrescriptionViewModel
            {
                PrescriptionId = p.PrescriptionId,
                AppointmentId = p.AppointmentId,
                MedicineName = p.MedicineName,
                Dosage = p.Dosage,
                Instructions = p.Instructions
            };
            ViewBag.Appointment = _appRepo.GetById(p.AppointmentId);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(PrescriptionViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Appointment = _appRepo.GetById(vm.AppointmentId);
                return View(vm);
            }
            _repo.Update(vm);
            return RedirectToAction(nameof(Index), new { appointmentId = vm.AppointmentId });
        }

        public IActionResult Delete(int id)
        {
            var p = _repo.GetById(id);
            if (p == null) return NotFound();
            return View(p);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var p = _repo.GetById(id);
            if (p == null) return NotFound();
            _repo.Delete(id);
            return RedirectToAction(nameof(Index), new { appointmentId = p.AppointmentId });
        }

        [HttpGet]
        public IActionResult AllPrescription() 
        {
            List<PrescriptionViewModel> prescriptions = new List<PrescriptionViewModel>();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                SqlCommand cmd = new SqlCommand("sp_GetAllPrescriptions", con);
                cmd.CommandType = CommandType.StoredProcedure;

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    prescriptions.Add(new PrescriptionViewModel
                    {
                        PrescriptionId = Convert.ToInt32(reader["PrescriptionId"]),
                        AppointmentId = Convert.ToInt32(reader["AppointmentId"]),
                        MedicineName = reader["MedicineName"].ToString(),
                        Dosage = reader["Dosage"].ToString(),
                        Instructions = reader["Instructions"].ToString(),
                        PrescribedOn = Convert.ToDateTime(reader["PrescribedOn"]),
                        PatientName = reader["PatientName"].ToString(),
                        DoctorName = reader["DoctorName"].ToString(),
                        AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"])
                    });
                }
            }

            return View(prescriptions);
        }


    }
}
