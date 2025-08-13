namespace DocBook.Models
{
    public class PrescriptionViewModel
    {
        public int PrescriptionId { get; set; }
        public int AppointmentId { get; set; }
        public string MedicineName { get; set; }
        public string Dosage { get; set; }
        public string Instructions { get; set; }
        public DateTime PrescribedOn { get; set; }
        public string PatientName { get; set; }  // Added
        public string DoctorName { get; set; }   // Added
        public DateTime AppointmentDate { get; set; }

    }
}
