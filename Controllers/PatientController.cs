using DocBook.Models;
using DocBook.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DocBook.Controllers
{
    public class PatientController : Controller
    {
        private readonly PatientRepository _repo;
        public PatientController(PatientRepository repo)
        {
            _repo = repo;
        }
        public IActionResult Index()
        {
            var patients = _repo.GetPatients();
            Console.WriteLine("Patients count: " + patients.Count);
            foreach (var p in patients)
                Console.WriteLine(p.Name);
            return View(patients);
        }

        public IActionResult Create() => View(new Patient());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Patient model)
        {
            if (!ModelState.IsValid)
            {
               return View(model);
               
            }
            _repo.AddPatient(model);
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var model = _repo.GetPatientById(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Patient model)
        {
            if (!ModelState.IsValid) return View(model);
            _repo.Update(model);
            return RedirectToAction(nameof(Index));
        }

       
        public IActionResult Delete(int id)
        {
            var model = _repo.GetPatientById(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repo.Delete(id);
            return RedirectToAction(nameof(Index));
        }


    }
}
