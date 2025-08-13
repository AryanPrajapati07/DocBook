namespace DocBook.Models
{
    public class AppointmentViewModel
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; } 
    }
}
