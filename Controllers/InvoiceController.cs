using DinkToPdf;
using DinkToPdf.Contracts;
using DocBook.Helpers;
using DocBook.Models;
using DocBook.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Runtime.CompilerServices;

namespace DocBook.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly InvoiceRepository _invoiceRepo;
        private readonly IConfiguration _config;
        private readonly string _cs;
        private readonly IConverter _converter;

        public InvoiceController(InvoiceRepository invoiceRepo, IConfiguration config, IConverter converter)
        {
            _invoiceRepo = invoiceRepo;
            _config = config;
            _cs = _config.GetConnectionString("DefaultConnection");
            _converter = converter;
        }

        public IActionResult Index()
        {
            var invoices = _invoiceRepo.GetAll();
            return View(invoices);
        }

        [HttpGet]
        public IActionResult Create(int appointmentId)
        {
            // Pull PatientId/DoctorId/Names/Date from Appointments
            var vm = LoadAppointmentForInvoice(appointmentId);
            if (vm == null) return NotFound("Appointment not found.");
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(InvoiceCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                // Ensure top info is reloaded if someone tampers
                var reloaded = LoadAppointmentForInvoice(vm.AppointmentId);
                if (reloaded != null)
                {
                    vm.PatientId = reloaded.PatientId;
                    vm.DoctorId = reloaded.DoctorId;
                    vm.PatientName = reloaded.PatientName;
                    vm.DoctorName = reloaded.DoctorName;
                    vm.AppointmentDate = reloaded.AppointmentDate;
                }
                return View(vm);
            }

            // Persist
            var invoice = new Invoice
            {
                AppointmentId = vm.AppointmentId,
                PatientId = vm.PatientId,
                DoctorId = vm.DoctorId,
                Services = vm.Services,
                TotalAmount = vm.TotalAmount
            };

            _invoiceRepo.Add(invoice);
            return RedirectToAction(nameof(Index));
        }

        // Details
        public IActionResult Details(int id)
        {
            var inv = _invoiceRepo.GetById(id);
            if (inv == null) return NotFound();
            return View(inv);
        }

        // Delete
        public IActionResult Delete(int id)
        {
            _invoiceRepo.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        // Helper: read appointment joins for reception workflow
        private InvoiceCreateViewModel LoadAppointmentForInvoice(int appointmentId)
        {
            using var con = new SqlConnection(_cs);
            using var cmd = new SqlCommand(@"
                SELECT a.AppointmentId,
                       a.AppointmentDate,
                       a.PatientId,
                       p.Name AS PatientName,
                       a.DoctorId,
                       d.Name AS DoctorName
                FROM Appointments a
                INNER JOIN Patients p ON p.PatientId = a.PatientId
                INNER JOIN Doctors d  ON d.DoctorId = a.DoctorId
                WHERE a.AppointmentId = @AppointmentId;", con);

            cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);
            con.Open();
            using var rd = cmd.ExecuteReader();
            if (!rd.Read()) return null;

            return new InvoiceCreateViewModel
            {
                AppointmentId = appointmentId,
                AppointmentDate = Convert.ToDateTime(rd["AppointmentDate"]),
                PatientId = Convert.ToInt32(rd["PatientId"]),
                PatientName = rd["PatientName"].ToString(),
                DoctorId = Convert.ToInt32(rd["DoctorId"]),
                DoctorName = rd["DoctorName"].ToString()
            };
        }


        public IActionResult Download(int Id)
        {
            InvoicePdfViewModel vm = null;
            using (SqlConnection con = new SqlConnection(_cs))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetInvoiceWithDetails", con))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@InvoiceId", Id);
                    con.Open();
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            vm = new InvoicePdfViewModel
                            {
                                InvoiceId = Convert.ToInt32(rd["InvoiceId"]),
                                PatientName = rd["PatientName"].ToString(),
                                DoctorName = rd["DoctorName"].ToString(),
                                AppointmentDate = Convert.ToDateTime(rd["AppointmentDate"]),
                                Services = rd["Services"] == DBNull.Value ? null : rd["Services"].ToString(),
                                TotalAmount = Convert.ToDecimal(rd["TotalAmount"]),
                                InvoiceDate = Convert.ToDateTime(rd["InvoiceDate"])
                            };
                        }
                    }
                }
            }

            if(vm == null)
            {
                return NotFound("Invoice not found.");
            }

            string html = RazorToStringRenderer.RenderViewToString(this, "InvoicePdf", vm);
            var pdfDoc = new HtmlToPdfDocument()
            {
                GlobalSettings = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings { Top = 10, Bottom = 10, Left = 10, Right = 10 }
                },
                Objects = {
                    new ObjectSettings
                    {
                        HtmlContent = html
                       
                    }
                }

            };

            var file = _converter.Convert(pdfDoc);
            return File(file, "application/pdf", $"Invoice_{vm.InvoiceId}.pdf");

        }

    }
}
