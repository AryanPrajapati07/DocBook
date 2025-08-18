using DocBook.Data;
using DocBook.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DocBook.Repositories
{
    public class DoctorRepository
    {
        private readonly AdoHelper _adoHelper;
        private readonly string _connectionString;

        public DoctorRepository(AdoHelper adoHelper , IConfiguration configuration)
        {
            _adoHelper = adoHelper;
            _connectionString = configuration.GetConnectionString("DefaultConnection");

        }

        public List<Doctor> GetDoctors()
        {
            var dt = _adoHelper.ExecDT("sp_GetAllDoctors");
            var list = new List<Doctor>();
            foreach (DataRow r in dt.Rows)
            {
                list.Add(new Doctor
                {
                    DoctorId = (int)r["DoctorId"],
                    Name = r["Name"].ToString(),
                    Specialization = r["Specialization"]?.ToString(),
                    Contact = r["Contact"]?.ToString()
                });
            }
            return list;
        }

      


        public Doctor GetDoctorById(int id)
        {
            var dt = _adoHelper.ExecDT(
                "sp_GetDoctorById",
                new SqlParameter("@DoctorId", id)
                );
            if (dt.Rows.Count == 0) return null;

            var r = dt.Rows[0];
            return new Doctor
            {
                DoctorId = (int)r["DoctorId"],
                Name = r["Name"].ToString(),
                Specialization = r["Specialization"]?.ToString(),
                Contact = r["Contact"]?.ToString()
            };
        }

        public void Add(Doctor doctor)
        {
            _adoHelper.ExecNonQuery(
                "sp_AddDoctor",
                new SqlParameter("@Name", doctor.Name ?? (object)DBNull.Value),
                new SqlParameter("@Specialization", doctor.Specialization ?? (object)DBNull.Value),
                new SqlParameter("@Contact", doctor.Contact ?? (object)DBNull.Value)
            );
        }

        public void Update(Doctor doctor)
        {
            _adoHelper.ExecNonQuery(
                "sp_UpdateDoctor",
                new SqlParameter("@DoctorId", doctor.DoctorId),
                new SqlParameter("@Name", doctor.Name ?? (object)DBNull.Value),
                new SqlParameter("@Specialization", doctor.Specialization ?? (object)DBNull.Value),
                new SqlParameter("@Contact", doctor.Contact ?? (object)DBNull.Value)
            );
        }

        public void Delete(int id)
        {
            _adoHelper.ExecNonQuery(
                "sp_DeleteDoctor",
                new SqlParameter("@DoctorId", id)
            );
        }

    }
}
