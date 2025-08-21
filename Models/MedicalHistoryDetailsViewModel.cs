namespace DocBook.Models
{
    public class MedicalHistoryDetailsViewModel
    {
        public MedicalHistory History { get; set; }
        public List<MedicalReport> Reports { get; set; }
        public IFormFile ReportFile { get; set; }
    }
}
