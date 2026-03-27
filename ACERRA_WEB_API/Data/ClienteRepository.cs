using Dapper;
using MySql.Data.MySqlClient;
using ACERRA_WEB_API.Models;

namespace ACERRA_WEB_API.Repository;

public class ClienteRepository
{
    private readonly string _connectionString;

    public ClienteRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    private MySqlConnection Connection()
        => new MySqlConnection(_connectionString);

    public async Task<IEnumerable<ClienteDto>> GetClientes()
    {
        using var db = Connection();

        var sql = @"SELECT id, nome FROM tb_cliente";

        var result = await db.QueryAsync<ClienteDto>(sql);

        return result;
    }
}