using Microsoft.Data.SqlClient;
using System.Data;

namespace DocBook.Repositories
{
    public class SymptomCheckerRepository
    {
        private readonly string _connectionString;

        public SymptomCheckerRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<Symptom>> GetAllSymptomsAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var symptoms = await connection.QueryAsync<Symptom>("sp_GetAllSymptoms", commandType : CommandType.StoredProcedure);
                return symptoms.ToList();
            }
        }

        public async Task<List<Disease>> GetDiseaseSuggetionAsync(string symptomIds)
        {
            using (var connection = new SqlConnection(_connectionString)) 
            {
                var diseases = await connection.QueryAsync<Disease>("sp_GetDiseaseSuggestions", new { SymptomIds = symptomIds }, commandType: CommandType.StoredProcedure);
                return diseases.ToList();
            }
        }

        public async Task LogAIInteractionAsync(string inputSymptoms, string suggestedDisease, decimal confidenceScore)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("sp_LogAIInteraction", new { InputSymptoms = inputSymptoms, SuggestedDisease = suggestedDisease, ConfidenceScore = confidenceScore }, commandType: CommandType.StoredProcedure);
            }
        }

    }
}
