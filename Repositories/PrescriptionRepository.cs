using DocBook.Data;
using DocBook.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DocBook.Repositories
{
    public class PrescriptionRepository
    {
        private readonly AdoHelper _ado;
        public PrescriptionRepository(AdoHelper ado)
        {
            _ado = ado;
        }

        public List<Prescription> GetByAppointment(int appointmentId)
        {
            var dt = _ado.ExecDT("sp_GetPrescriptionByAppointment", new SqlParameter("@AppointmentId", appointmentId));
            var list = new List<Prescription>();
            foreach (DataRow r in dt.Rows)
            {
                list.Add(new Prescription
                {
                    PrescriptionId = Convert.ToInt32(r["PrescriptionId"]),
                    AppointmentId = Convert.ToInt32(r["AppointmentId"]),
                    MedicineName = r["MedicineName"].ToString(),
                    Dosage = r["Dosage"] == DBNull.Value ? null : r["Dosage"].ToString(),
                    Instructions = r["Instructions"] == DBNull.Value ? null : r["Instructions"].ToString(),
                    PrescribedOn = Convert.ToDateTime(r["PrescribedOn"])
                });
            }
            return list;
        }

        public Prescription GetById(int id)
        {
            var dt = _ado.ExecDT("sp_GetPrescriptionById", new SqlParameter("@PrescriptionId", id));
            if (dt.Rows.Count == 0) return null;
            var r = dt.Rows[0];
            return new Prescription
            {
                PrescriptionId = (int)r["PrescriptionId"],
                AppointmentId = (int)r["AppointmentId"],
                MedicineName = r["MedicineName"].ToString(),
                Dosage = r["Dosage"] == DBNull.Value ? null : r["Dosage"].ToString(),
                Instructions = r["Instructions"] == DBNull.Value ? null : r["Instructions"].ToString(),
                PrescribedOn = (DateTime)r["PrescribedOn"]
            };
        }

        public void Add(PrescriptionViewModel vm)
        {
            _ado.ExecNonQuery("sp_AddPrescription",
                new SqlParameter("@AppointmentId", vm.AppointmentId),
                new SqlParameter("@MedicineName", vm.MedicineName ?? (object)DBNull.Value),
                new SqlParameter("@Dosage", vm.Dosage ?? (object)DBNull.Value),
                new SqlParameter("@Instructions", vm.Instructions ?? (object)DBNull.Value)
            );
        }

        public void Update(PrescriptionViewModel vm)
        {
            _ado.ExecNonQuery("sp_UpdatePrescription",
                new SqlParameter("@PrescriptionId", vm.PrescriptionId),
                new SqlParameter("@MedicineName", vm.MedicineName ?? (object)DBNull.Value),
                new SqlParameter("@Dosage", vm.Dosage ?? (object)DBNull.Value),
                new SqlParameter("@Instructions", vm.Instructions ?? (object)DBNull.Value)
            );
        }

        public void Delete(int id)
        {
            _ado.ExecNonQuery("sp_DeletePrescription", new SqlParameter("@PrescriptionId", id));
        }

        



    }
}
