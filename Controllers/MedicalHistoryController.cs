using DocBook.Models;
using DocBook.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace DocBook.Controllers
{
    public class MedicalHistoryController : Controller
    {
        private readonly string _connectionString;
        private readonly MedicalHistoryRepository _repo;
        public MedicalHistoryController(MedicalHistoryRepository repo, IConfiguration configuration)
        {
            _repo = repo;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
           
        }
        public IActionResult Index(int patientId)
        {
            
           
            MedicalHistory history = null;
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_GetMedicalHistoryByPatient", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PatientId", patientId);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    history = new MedicalHistory
                    {
                        HistoryId = Convert.ToInt32(reader["HistoryId"]),
                        PatientId = Convert.ToInt32(reader["PatientId"]),
                        Diagnosis = reader["Diagnosis"].ToString(),
                        Medications = reader["Medications"].ToString(),
                        Allergies = reader["Allergies"].ToString(),
                        LastUpdated = Convert.ToDateTime(reader["LastUpdated"])
                    };
                }   
            }
            return View(history ?? new MedicalHistory { PatientId = patientId });
        }

        [HttpPost]
        public IActionResult Save(MedicalHistory history)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_SaveMedicalHistory", conn);
                conn.Open();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PatientId", history.PatientId);
                cmd.Parameters.AddWithValue("@Diagnosis", history.Diagnosis);
                cmd.Parameters.AddWithValue("@Medications", history.Medications);
                cmd.Parameters.AddWithValue("@Allergies", history.Allergies);
                //cmd.Parameters.AddWithValue("@LastUpdated", DateTime.Now);
                Console.WriteLine($"Inserting MedicalHistory for PatientId = {history.PatientId}");

                cmd.ExecuteNonQuery();
            


            }
            return RedirectToAction("Index", new { patientId = history.PatientId });

        }

       
        public IActionResult ViewHistory()
        {
            var history = _repo.GetAllMedicalHistories();
            return View(history);
        }


    }
}
