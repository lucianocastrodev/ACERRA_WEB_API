using Dapper;
using MySql.Data.MySqlClient;
using ACERRA_WEB_API.Models;
using System.Data;

namespace ACERRA_WEB_API.Data
{
    public class BalancaRepository
    {
        private readonly string _connectionString;

        public BalancaRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }
        private IDbConnection Connection()
            => new MySqlConnection(_connectionString);

        // Retorna todas as balanças
        public async Task<IEnumerable<Balanca>> GetAll(string setor)
        {
            using var db = Connection();
            var sql = @"SELECT * FROM tb_balanca WHERE setor = @Setor";

            return await db.QueryAsync<Balanca>(sql, new { Setor = setor });
        }
    }
}
