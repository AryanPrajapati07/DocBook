using DocBook.Data;
using DocBook.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DocBook.Repositories
{
    public class AppointmentRepository
    {
        private readonly AdoHelper _adoHelper;
        public AppointmentRepository(AdoHelper adoHelper)
        {
            _adoHelper = adoHelper;
        }

        public List<Appointment> GetAll()
        {
            var dt = _adoHelper.ExecDT("sp_GetAllAppointments");
            var list = new List<Appointment>();
            foreach (DataRow r in dt.Rows)
            {
                list.Add(new Appointment
                {
                    AppointmentId = Convert.ToInt32(r["AppointmentId"]),
                    PatientId = Convert.ToInt32(r["PatientId"]),
                    PatientName = r["PatientName"].ToString(),
                    DoctorId = Convert.ToInt32(r["DoctorId"]),
                    DoctorName = r["DoctorName"].ToString(),
                    AppointmentDate = Convert.ToDateTime(r["AppointmentDate"]),
                    Reason = r["Reason"] == DBNull.Value ? null : r["Reason"].ToString(),
                    Status = r["Status"] == DBNull.Value ? null : r["Status"].ToString()
                });
            }
            return list;

        }

        public Appointment GetById(int id)
        {
            var dt = _adoHelper.ExecDT("sp_GetAppointmentById", new SqlParameter("@AppointmentId", id));
            if (dt.Rows.Count == 0) return null;
            var r = dt.Rows[0];
            return new Appointment
            {
                AppointmentId = Convert.ToInt32(r["AppointmentId"]),
                PatientId = Convert.ToInt32(r["PatientId"]),
                DoctorId = Convert.ToInt32(r["DoctorId"]),
                AppointmentDate = Convert.ToDateTime(r["AppointmentDate"]),
                Reason = r["Reason"] == DBNull.Value ? null : r["Reason"].ToString(),
                Status = r["Status"] == DBNull.Value ? null : r["Status"].ToString()
            };
        }

        public void Add(AppointmentViewModel appointment)
        {
            _adoHelper.ExecNonQuery("sp_AddAppointment",
                new SqlParameter("@PatientId", appointment.PatientId),
                new SqlParameter("@DoctorId", appointment.DoctorId),
                new SqlParameter("@AppointmentDate", appointment.AppointmentDate),
                new SqlParameter("@Reason", appointment.Reason ?? (object)DBNull.Value),
                new SqlParameter("@Status", appointment.Status ?? (object)DBNull.Value)
            );
        }

        public void Update(AppointmentViewModel appointment)
        {
            _adoHelper.ExecNonQuery("sp_UpdateAppointment",
                new SqlParameter("@AppointmentId", appointment.AppointmentId),
                new SqlParameter("@PatientId", appointment.PatientId),
                new SqlParameter("@DoctorId", appointment.DoctorId),
                new SqlParameter("@AppointmentDate", appointment.AppointmentDate),
                new SqlParameter("@Reason", appointment.Reason ?? (object)DBNull.Value),
                new SqlParameter("@Status", appointment.Status ?? (object)DBNull.Value)
            );
        }

        public void Delete(int id)
        {
            _adoHelper.ExecNonQuery("sp_DeleteAppointment", new SqlParameter("@AppointmentId", id));

        }

    }
}
