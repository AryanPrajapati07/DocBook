namespace DocBook.Models
{
    public class PrescriptionPdfViewModel
    {
        public int PrescriptionId { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string MedicineName { get; set; }
        public string Instructions { get; set; }
    }
}
