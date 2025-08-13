namespace DocBook.Models
{
    public class Prescription
    {
        public int PrescriptionId { get; set; }
        public int AppointmentId { get; set; }
        public string MedicineName { get; set; }
        public string Dosage { get; set; }
        public string Instructions { get; set; }
        public DateTime PrescribedOn { get; set; }
    }
}
