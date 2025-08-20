using DocBook.Data;
using DocBook.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DocBook.Repositories
{
    public class InvoiceRepository
    {
        private readonly AdoHelper _adoHelper;
        public InvoiceRepository(AdoHelper adoHelper)
        {
            _adoHelper = adoHelper;
        }

        public void Add(Invoice invoice)
        {
            _adoHelper.ExecNonQuery("sp_AddInvoice",
                new SqlParameter("@AppointmentId", invoice.AppointmentId),
                new SqlParameter("@PatientId", invoice.PatientId),
                new SqlParameter("@DoctorId", invoice.DoctorId),
                new SqlParameter("@Services", invoice.Services ?? (object)DBNull.Value),
                new SqlParameter("@TotalAmount", invoice.TotalAmount)
                //new SqlParameter("@InvoiceDate", invoice.InvoiceDate)
            );
        }

        public List<Invoice> GetAll()
        {
            var dt = _adoHelper.ExecDT("sp_GetAllInvoices");
            var list = new List<Invoice>();
            foreach (DataRow r in dt.Rows)
            {
                list.Add(new Invoice
                {
                    InvoiceId = (int)r["InvoiceId"],
                    AppointmentId = (int)r["AppointmentId"],
                    PatientId = (int)r["PatientId"],
                    DoctorId = (int)r["DoctorId"],
                    PatientName = r["PatientName"].ToString(),
                    DoctorName = r["DoctorName"].ToString(),
                    AppointmentDate = Convert.ToDateTime(r["AppointmentDate"]),
                    Services = r["Services"] == DBNull.Value ? null : r["Services"].ToString(),
                    TotalAmount = Convert.ToDecimal(r["TotalAmount"]),
                    InvoiceDate = Convert.ToDateTime(r["InvoiceDate"])
                });
            }
            return list;
        }

        public Invoice GetById(int id)
        {
            var dt = _adoHelper.ExecDT("sp_GetInvoiceById", new SqlParameter("@InvoiceId", id));
            if (dt.Rows.Count == 0) return null;
            var r = dt.Rows[0];
            return new Invoice
            {
                InvoiceId = (int)r["InvoiceId"],
                AppointmentId = (int)r["AppointmentId"],
                PatientId = (int)r["PatientId"],
                DoctorId = (int)r["DoctorId"],
                PatientName = r["PatientName"].ToString(),
                DoctorName = r["DoctorName"].ToString(),
                AppointmentDate = Convert.ToDateTime(r["AppointmentDate"]),
                Services = r["Services"] == DBNull.Value ? null : r["Services"].ToString(),
                TotalAmount = Convert.ToDecimal(r["TotalAmount"]),
                InvoiceDate = Convert.ToDateTime(r["InvoiceDate"])
            };
        }


        public void Delete(int id)
        {
            _adoHelper.ExecNonQuery("sp_DeleteInvoice", new SqlParameter("@InvoiceId", id));
        }
    }
}
