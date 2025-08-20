using System.ComponentModel.DataAnnotations;

namespace DocBook.Models
{
    public class InvoiceCreateViewModel
    {
        [Required]
        public int AppointmentId { get; set; }

        // Hidden (auto-filled from appointment)
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public DateTime AppointmentDate { get; set; }

        // Form fields
        [Display(Name = "Services")]
        public string Services { get; set; }  // comma-separated text

        [Required, Range(0, 999999)]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }
    }
}
