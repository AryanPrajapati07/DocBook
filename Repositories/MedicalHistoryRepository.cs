using DocBook.Data;
using DocBook.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DocBook.Repositories
{
    public class MedicalHistoryRepository
    {
        private readonly AdoHelper _adoHelper;
        private readonly string _connectionString;

        public MedicalHistoryRepository(AdoHelper adoHelper, IConfiguration config)
        {
            _adoHelper = adoHelper;
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public void Save(MedicalHistory history)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // ✅ First check if PatientId exists in Patients table
                SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Patients WHERE PatientId = @PatientId", conn);
                checkCmd.Parameters.AddWithValue("@PatientId", history.PatientId);

                int exists = (int)checkCmd.ExecuteScalar();
                if (exists == 0)
                {
                    throw new Exception($"Patient with ID {history.PatientId} does not exist in Patients table.");
                }

                // ✅ Now insert history
                SqlCommand cmd = new SqlCommand("sp_SaveMedicalHistory", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@PatientId", history.PatientId);
                cmd.Parameters.AddWithValue("@Diagnosis", history.Diagnosis ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Medications", history.Medications ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Allergies", history.Allergies ?? (object)DBNull.Value);

                cmd.ExecuteNonQuery();
            }
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

        public List<MedicalHistory> GetByPatientId(int patientId)
        {
            var histories = new List<MedicalHistory>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT HistoryId, PatientId, Diagnosis, Medications, Allergies FROM MedicalHistory WHERE PatientId = @PatientId", conn);
                cmd.Parameters.AddWithValue("@PatientId", patientId);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    histories.Add(new MedicalHistory
                    {
                        HistoryId = (int)reader["HistoryId"],
                        PatientId = (int)reader["PatientId"],
                        Diagnosis = reader["Diagnosis"].ToString(),
                        Medications = reader["Medications"].ToString(),
                        Allergies = reader["Allergies"].ToString()
                    });
                }
            }

            return histories;
        }

        



    }
}
