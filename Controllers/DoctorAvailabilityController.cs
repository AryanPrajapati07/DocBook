using DocBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DocBook.Controllers
{
    public class DoctorAvailabilityController : Controller
    {
        private readonly string _connectionString;
        public DoctorAvailabilityController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public IActionResult SetAvailability(int doctorId)
        {
            var model = new DoctorAvailability
            {
                DoctorId = doctorId
            };
            return View(model);
        }


        [HttpPost]
        public IActionResult SetAvailability(DoctorAvailability model)
        {
            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("sp_AddOrUpdateDoctorAvailability", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@DoctorId", model.DoctorId);
                cmd.Parameters.AddWithValue("@AvailableDate", model.AvailableDate);
                cmd.Parameters.AddWithValue("@StartTime", model.StartTime);
                cmd.Parameters.AddWithValue("@EndTime", model.EndTime);
                cmd.Parameters.AddWithValue("@SlotDurationMinutes", model.SlotDurationMinutes);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            TempData["Message"] = "Availability saved successfully.";
            return RedirectToAction("SetAvailability");
        }

    }
}
