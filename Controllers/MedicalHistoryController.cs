using DocBook.Models;
using DocBook.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace DocBook.Controllers
{
    public class MedicalHistoryController : Controller
    {
        private readonly MedicalHistoryRepository _repo;
        private readonly string _connectionString;

        public MedicalHistoryController(MedicalHistoryRepository repo, IConfiguration config)
        {
            _repo = repo;
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public IActionResult Index(int patientId)
        {
            var histories = _repo.GetByPatientId(patientId);  // should return List<MedicalHistory>
            return View(histories);  // ✅ returning IEnumerable
        }


        [HttpPost]
        public IActionResult Save(MedicalHistory history)
        {
            if (!ModelState.IsValid)
                return View("Index", history);

            _repo.Save(history);
            return RedirectToAction("Index", new { patientId = history.PatientId });
        }

        public IActionResult ViewHistory()
        {
            var history = _repo.GetAllMedicalHistories();
            return View(history);
        }


        [HttpPost]
        public IActionResult UploadReport(int historyId, int patientId, IFormFile ReportFile)
        {
            if (ReportFile != null && ReportFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "reports");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ReportFile.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ReportFile.CopyTo(stream);
                }

                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("sp_AddMedicalReport", con);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PatientId", patientId);
                    cmd.Parameters.AddWithValue("@HistoryId", historyId);
                    cmd.Parameters.AddWithValue("@ReportName", ReportFile.FileName);    
                    cmd.Parameters.AddWithValue("@FilePath", "/reports" + fileName);
                    cmd.ExecuteNonQuery();
                }

            }
            return RedirectToAction("Details", new {id=historyId});

        }

        public IActionResult Details(int id)
        {
            MedicalHistoryDetailsViewModel vm = new MedicalHistoryDetailsViewModel();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM MedicalHistory WHERE HistoryId = @id", con);
                cmd.Parameters.AddWithValue("@id", id);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        vm.History = new MedicalHistory
                        {
                            PatientId = (int)reader["PatientId"],
                            HistoryId = (int)reader["HistoryId"],
                            Diagnosis = reader["Diagnosis"].ToString(),
                            Allergies = reader["Allergies"]?.ToString()
                        };
                    }
                }
            }

            //load reports
            vm.Reports = new List<MedicalReport>();
            using (SqlConnection con = new SqlConnection(_connectionString)) 
            { 
                con.Open();
                SqlCommand cmd = new SqlCommand("sp_GetMedicalReports", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@HistoryId", id);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        vm.Reports.Add(new MedicalReport
                        {
                            ReportId = (int)reader["ReportId"],
                            ReportName = reader["ReportName"].ToString(),
                            FilePath = reader["FilePath"].ToString(),
                            UploadedOn = (DateTime)reader["UploadedOn"]
                        });
                    }
                }

            }
            return View(vm);

        }




    }
}
