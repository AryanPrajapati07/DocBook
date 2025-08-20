using DocBook.Models;
using DocBook.Repositories;
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
    }

}
