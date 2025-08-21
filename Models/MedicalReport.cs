using System.Data;

namespace DocBook.Models
{
    public class MedicalReport
    {
        public int ReportId { get; set; }
        public int PatientId { get; set; }
        public int HistoryId { get; set; }
        public string ReportName { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadedOn { get; set; }
    }
}
