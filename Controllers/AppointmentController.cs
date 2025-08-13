using DocBook.Models;
using DocBook.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DocBook.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly AppointmentRepository _repo;
        private readonly PatientRepository _patientRepo;
        private readonly DoctorRepository _doctorRepo;
        public AppointmentController(AppointmentRepository repo, PatientRepository patientRepo, DoctorRepository doctorRepo)
        {
            _repo = repo;
            _patientRepo = patientRepo;
            _doctorRepo = doctorRepo;
        }
        
        public IActionResult Index()
        {
            var list = _repo.GetAll();
            return View(list);
        }

        public IActionResult Create() 
        {
            PopulatePatientsAndDoctors();
            var vm = new AppointmentViewModel
            {
                AppointmentDate = DateTime.Now

            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AppointmentViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                PopulatePatientsAndDoctors();
                return View(vm);
            }
            _repo.Add(vm);
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var model = _repo.GetById(id);
            if (model == null) return NotFound();
            PopulatePatientsAndDoctors();
            var vm = new AppointmentViewModel
            {
                AppointmentId = model.AppointmentId,
                PatientId = model.PatientId,
                DoctorId = model.DoctorId,
                AppointmentDate = model.AppointmentDate,
                Reason = model.Reason,
                Status = model.Status
            };
            PopulatePatientsAndDoctors();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(AppointmentViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                PopulatePatientsAndDoctors();
                return View(vm);
            }
            _repo.Update(vm);
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var model = _repo.GetById(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repo.Delete(id);
            return RedirectToAction("Index");
        }

        private void PopulatePatientsAndDoctors()
        {
            var patients = _patientRepo.GetPatients();
            var doctors = _doctorRepo.GetDoctors();
            ViewBag.Patients = new SelectList(patients, "PatientId", "Name");
            ViewBag.Doctors = new SelectList(doctors, "DoctorId", "Name");
        }

       

    }
}
