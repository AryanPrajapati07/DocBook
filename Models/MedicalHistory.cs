namespace DocBook.Models
{
    public class MedicalHistory
    {
        public int HistoryId { get; set; }
        public int PatientId { get; set; }
        public string Diagnosis { get; set; }
        public string Medications { get; set; }
        public string Allergies { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
