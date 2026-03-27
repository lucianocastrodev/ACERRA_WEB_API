using Dapper;
using MySql.Data.MySqlClient;
using ACERRA_WEB_API.Models;
using System.Data;

namespace ACERRA_WEB_API.Data;

public class MaterialRepository
{
    private readonly string _connectionString;

    public MaterialRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException("Connection string 'DefaultConnection' não encontrada.");
    }

    private IDbConnection Connection()
        => new MySqlConnection(_connectionString);

    // Retorna todos os produtos
    public async Task<IEnumerable<Material>> GetAll()
    {
        using var db = Connection();
        var sql = @"
            SELECT 
                id, descricao, preco_compra AS PrecoCompra, preco_venda AS PrecoVenda
            FROM tb_material
            ORDER BY id DESC";

        return await db.QueryAsync<Material>(sql);
    }

    // Retorna um produto pelo ID
    public async Task<Material?> GetById(int id)
    {
        using var db = Connection();
        var sql = @"
            SELECT 
               id, descricao, preco_compra AS PrecoCompra, preco_venda AS PrecoVenda
            FROM tb_material
            WHERE id = @Id";

        return await db.QueryFirstOrDefaultAsync<Material>(sql, new { Id = id });
    }

    // Cria um novo produto e retorna o ID
    public async Task<int> Create(Material produto)
    {
        using var db = Connection();
        var sql = @"
            INSERT INTO tb_material
                (descricao)
            VALUES 
                (@Descricao);
            
            SELECT LAST_INSERT_ID();";

        return await db.ExecuteScalarAsync<int>(sql, produto);
    }

    // Atualiza um produto existente
    public async Task<bool> Update(Material produto)
    {
        using var db = Connection();
        var sql = @"
            UPDATE tb_material SET
                descricao = @Descricao
            WHERE id = @Id;";

        var rows = await db.ExecuteAsync(sql, produto);
        return rows > 0;
    }

    // Remove um produto pelo ID
    public async Task<bool> Delete(int id)
    {
        using var db = Connection();
        var sql = "DELETE FROM tb_material WHERE id = @Id";
        var rows = await db.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }
}