using DocBook.Models;
using DocBook.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DocBook.Controllers
{
    public class DoctorController : Controller
    {
        private readonly DoctorRepository _repo;
        public DoctorController(DoctorRepository repo)
        {
            _repo = repo;
        }
        public IActionResult Index()  
        {
            var doctors = _repo.GetDoctors();
            return View(doctors);
        }

        public IActionResult Create() 
        {             
            return View();
        }

        [HttpPost]
        public IActionResult Create(Models.Doctor doctor)
        {
            if (ModelState.IsValid)
            {
                _repo.Add(doctor);
                return RedirectToAction("Index");
            }
            return View(doctor);
        }

        public IActionResult Edit(int id) => View(_repo.GetDoctorById(id));

        [HttpPost]
        public IActionResult Edit(Doctor doctor)
        {
            if (ModelState.IsValid)
            {
                _repo.Update(doctor);
                return RedirectToAction("Index");
            }
            return View(doctor);
        }

        public IActionResult Delete(int id)
        {
            var doctor = _repo.GetDoctorById(id);
            if (doctor == null) return NotFound();
            return View(doctor);
        }


    }
}
