using DocBook.Helpers;
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
            PrescriptionViewModel model = new PrescriptionViewModel();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM Prescriptions WHERE PrescriptionId = @Id", con);
                cmd.Parameters.AddWithValue("@Id", id);
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.Read())
                {
                    model.PrescriptionId = Convert.ToInt32(rdr["PrescriptionId"]);
                    model.MedicineName = rdr["MedicineName"].ToString();
                    model.Dosage = rdr["Dosage"].ToString();
                    model.Instructions = rdr["Instructions"].ToString();
                    
                }
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(PrescriptionViewModel model)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("dbo.sp_UpdatePrescription", con);
                cmd.CommandType = CommandType.StoredProcedure;


                cmd.Parameters.AddWithValue("@PrescriptionId", model.PrescriptionId);
                cmd.Parameters.AddWithValue("@MedicineName", model.MedicineName);
                cmd.Parameters.AddWithValue("@Dosage", (object)model.Dosage ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Instructions", (object)model.Instructions ?? DBNull.Value);



                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("AllPrescription");
        }


        public IActionResult Delete(int id)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_DeletePrescription", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PrescriptionId", id);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("AllPrescription");
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



        public IActionResult Download(int id) 
        {
            PrescriptionPdfViewModel model = null;

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetPreWithNames", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PrescriptionId", id);
                    con.Open();
                    using(SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model = new PrescriptionPdfViewModel
                            {
                                PrescriptionId = Convert.ToInt32(reader["PrescriptionId"]),
                                PatientName = reader["PatientName"].ToString(),
                                DoctorName = reader["DoctorName"].ToString(),
                                AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"]),
                                MedicineName = reader["MedicineName"].ToString(),
                                Instructions = reader["Instructions"].ToString()
                            };
                        }
                    }
                }
            }

            if (model == null)
            {
                return NotFound("Prescription not found.");
            }

            string html = RazorToStringRenderer.RenderViewToString(this, "PrescriptionPdf", model);
            var Renderer = new HtmlToPdf();
            var pdf = Renderer.RenderHtmlAsPdf(html);
            return File(pdf.BinaryData, "application/pdf", $"Prescription_{model.PrescriptionId}.pdf");


        }


    }
}
