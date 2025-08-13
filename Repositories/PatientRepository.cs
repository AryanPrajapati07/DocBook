using DocBook.Data;
using DocBook.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DocBook.Repositories
{
    public class PatientRepository
    {
        private readonly AdoHelper _adoHelper;

        public PatientRepository(AdoHelper adoHelper)
        {
            _adoHelper = adoHelper;
        }

        public List<Patient> GetPatients()
        {
            var dt = _adoHelper.ExecDT("sp_GetAllPatients") ?? new DataTable();
            var list = new List<Patient>();

            foreach (DataRow r in dt.Rows)
            {
                list.Add(new Patient
                {
                    PatientId = r["PatientId"] != DBNull.Value ? Convert.ToInt32(r["PatientId"]) : 0,
                    Name = r["Name"]?.ToString(),
                    Gender = r["Gender"]?.ToString(),
                    DOB = r["DOB"] == DBNull.Value ? null : (DateTime?)r["DOB"],
                    Contact = r["Contact"]?.ToString(),
                    Address = r["Address"]?.ToString()
                });
            }

            return list;
        }

        public Patient GetPatientById(int id)
        {
            var dt = _adoHelper.ExecDT(
                "sp_GetPatientById",
                new SqlParameter("@PatientId", SqlDbType.Int) { Value = id }
            );

            if (dt.Rows.Count == 0) return null;
            var r = dt.Rows[0];
            return new Patient
            {
                PatientId = (int)r["PatientId"],
                Name = r["Name"].ToString(),
                Gender = r["Gender"]?.ToString(),
                DOB = r["DOB"] == DBNull.Value ? null : (DateTime?)r["DOB"],
                Contact = r["Contact"]?.ToString(),
                Address = r["Address"]?.ToString()
            };
        }


        public void AddPatient(Patient p)
        {
            _adoHelper.ExecNonQuery("sp_AddPatient",
                new SqlParameter("@Name", p.Name ?? (object)DBNull.Value),
                new SqlParameter("@Gender", p.Gender ?? (object)DBNull.Value),
                new SqlParameter("@DOB", p.DOB ?? (object)DBNull.Value),
                new SqlParameter("@Contact", p.Contact ?? (object)DBNull.Value),
                new SqlParameter("@Address", p.Address ?? (object)DBNull.Value)
            );
        }

        public void Update(Patient p)
        {
            _adoHelper.ExecNonQuery("sp_UpdatePatient",
                new SqlParameter("@PatientId", p.PatientId),
                new SqlParameter("@Name", p.Name ?? (object)DBNull.Value),
                new SqlParameter("@Gender", p.Gender ?? (object)DBNull.Value),
                new SqlParameter("@DOB", p.DOB ?? (object)DBNull.Value),
                new SqlParameter("@Contact", p.Contact ?? (object)DBNull.Value),
                new SqlParameter("@Address", p.Address ?? (object)DBNull.Value)
            );
        }

        public void Delete(int id)
        {
            _adoHelper.ExecNonQuery("sp_DeletePatient", new SqlParameter("@PatientId", id));
        }

    }
}
