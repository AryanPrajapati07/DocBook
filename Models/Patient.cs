namespace DocBook.Models
{
    public class Patient
    {
        public int PatientId { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public DateTime? DOB { get; set; }
        public string Contact { get; set; }
        public string Address { get; set; }
    }
}
