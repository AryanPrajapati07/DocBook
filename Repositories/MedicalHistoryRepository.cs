using DocBook.Data;
using DocBook.Models;
using System.Data;

namespace DocBook.Repositories
{
    public class MedicalHistoryRepository
    {

        private readonly AdoHelper _adoHelper;
        public MedicalHistoryRepository(AdoHelper adoHelper)
        {
            _adoHelper = adoHelper;
        }

        public List<MedicalHistory> GetAllMedicalHistories()
        {
            var dt = _adoHelper.ExecDT("sp_ViewHistory");
            var list = new List<MedicalHistory>();
            foreach (DataRow r in dt.Rows)
            {
                list.Add(new MedicalHistory
                {
                    HistoryId = Convert.ToInt32(r["HistoryId"]),
                    PatientId = Convert.ToInt32(r["PatientId"]),
                    Diagnosis = r["Diagnosis"].ToString(),
                    Medications = r["Medications"]?.ToString(),
                    Allergies = r["Allergies"]?.ToString()
                });
            }
            return list;
        }
    }
}
