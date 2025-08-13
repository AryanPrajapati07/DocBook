using Microsoft.Data.SqlClient;
using System.Data;

namespace DocBook.Data
{
    public class AdoHelper
    {
        private readonly string _connectionString;
        public AdoHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public DataTable ExecDT(string spName, params SqlParameter[] parameters)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(spName, con);
            cmd.CommandType = CommandType.StoredProcedure;
            if (parameters != null)
                cmd.Parameters.AddRange(parameters);
            using var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public int ExecNonQuery(string sp, params SqlParameter[] pars)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sp, con)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (pars != null)
            {
                cmd.Parameters.AddRange(pars);
            }
            con.Open();
            return cmd.ExecuteNonQuery();
        }
    }
}
