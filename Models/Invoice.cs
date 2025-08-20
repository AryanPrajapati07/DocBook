namespace DocBook.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }

        public string Services { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime InvoiceDate { get; set; }

        // Joined display fields
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public DateTime AppointmentDate { get; set; }
    }
}
