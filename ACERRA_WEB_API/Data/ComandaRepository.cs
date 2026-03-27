using Dapper;
using MySql.Data.MySqlClient;
using ACERRA_WEB_API.Models;
using System.Data;

namespace ACERRA_WEB_API.Data;

public class ComandaRepository
{
    private readonly string _connectionString;

    public ComandaRepository(IConfiguration configuration)
    {
        // Pego a string de conexão do appsettings
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    // Crio uma conexão nova sempre que precisar acessar o banco
    private IDbConnection Connection()
        => new MySqlConnection(_connectionString);

    public async Task<ComandaDto?> GetComanda(string codigoBarras, string setor)
    {
        using var db = Connection();

        // Busco a última comanda vinculada ao código de barras e setor
        var sql = @"
        SELECT 
            c.id, 
            c.descricao, 
            c.operacao,
            c.codigo_barras AS CodigoBarras,
            p.id AS ComandaPesagemId,
            COALESCE(p.status_id, 3) AS Status
        FROM tb_comanda c
        LEFT JOIN tb_comanda_pesagem p 
            ON p.comanda_id = c.id
        WHERE c.codigo_barras = @CodigoBarras 
          AND c.setor = @Setor
        ORDER BY p.id DESC
        LIMIT 1
    ";

        return await db.QueryFirstOrDefaultAsync<ComandaDto>(sql, new
        {
            CodigoBarras = codigoBarras,
            Setor = setor
        });
    }

    public async Task<int> Create(ComandaCreateDto dto)
    {
        using var db = Connection();

        // Insiro uma nova comanda de pesagem e retorno o ID gerado
        var sql = @"
        INSERT INTO tb_comanda_pesagem 
            (operacao, setor, veiculo_placa, veiculo_descricao, comanda_id, cliente_id, status_id, data_inicio, observacoes)
        VALUES 
            (@Operacao, @Setor, @VeiculoPlaca, @VeiculoDescricao, @ComandaId, @ClienteId, @Status, NOW(), @Observacoes);

        SELECT LAST_INSERT_ID();
    ";

        return await db.ExecuteScalarAsync<int>(sql, new
        {
            dto.Operacao,
            dto.Setor,
            dto.VeiculoPlaca,
            dto.VeiculoDescricao,
            ComandaId = dto.Id, // aqui faço o mapeamento manual porque o nome é diferente
            dto.ClienteId,
            dto.Status,
            dto.Observacoes
        });
    }

    public async Task<bool> ExisteComandaAberta(int comandaId)
    {
        using var db = Connection();

        // Verifico se já existe comanda aberta (status 1)
        var sql = @"
            SELECT id 
            FROM tb_comanda_pesagem 
            WHERE comanda_id = @ComandaId 
            AND status_id = 1
            LIMIT 1
        ";

        var result = await db.QueryFirstOrDefaultAsync<int?>(sql, new
        {
            ComandaId = comandaId
        });

        // Se encontrou algum registro, significa que existe aberta
        return result.HasValue;
    }

    // Lista comandas por setor (limitado a 100 registros)
    public async Task<List<Comanda>> GetComandas(string setor)
    {
        using var db = Connection();

        var sql = @"
            SELECT 
                c.id AS Id,
                p.id AS ComandaPesagemId,
                c.descricao AS Descricao,
                c.operacao AS Operacao,
                p.setor AS Setor,
                c.codigo_barras AS CodigoBarras,
                COALESCE(p.status_id, 3) AS Status,
                p.cliente_id AS ClienteId,
                cli.nome AS ClienteNome,
                p.veiculo_placa AS VeiculoPlaca,
                p.veiculo_descricao AS VeiculoDescricao
            FROM tb_comanda_pesagem p
            INNER JOIN tb_comanda c
                ON p.comanda_id = c.id
            LEFT JOIN tb_cliente cli
                ON p.cliente_id = cli.id
            WHERE p.setor = @Setor
            ORDER BY p.id DESC
            LIMIT 100
        ";

        var result = await db.QueryAsync<Comanda>(sql, new
        {
            Setor = setor
        });

        return result.ToList();
    }

    public async Task<ComandaDetalhes?> GetDetalhes(int id)
    {
        using var db = Connection();

        // Query principal com dados da comanda
        var sql = @"
            SELECT
                c.id AS ComandaId,
                c.codigo_barras AS ComandaCodigoBarras,
                c.descricao AS Comanda,
                cl.nome AS Cliente,
                p.operacao,
                p.veiculo_placa AS VeiculoPlaca,
                p.veiculo_descricao AS VeiculoDescricao,
                p.data_inicio AS DataInicio,
                p.status_id AS StatusComanda
            FROM tb_comanda_pesagem p
            LEFT JOIN tb_cliente cl ON cl.id = p.cliente_id
            LEFT JOIN tb_comanda c ON c.id = p.comanda_id
            WHERE p.id = @Id
            LIMIT 1
        ";

        var dados = await db.QueryFirstOrDefaultAsync(sql, new { Id = id });

        // Se não encontrou a comanda
        if (dados == null)
            return null;

        // Query dos itens vinculados à comanda
        var itensSql = @"
            SELECT
                m.descricao AS Descricao,
                pe.id,
                pe.status_id AS StatusId,
                pe.peso_bruto AS PesoBruto,
                pe.tara AS Tara,
                pe.quantidade AS Quantidade,
                CONCAT(f1.nome, ' ', f1.sobrenome) AS FuncionarioPesoBruto,
                CONCAT(f2.nome, ' ', f2.sobrenome) AS FuncionarioTara
            FROM tb_comanda_pesagem cp
            INNER JOIN tb_pesagem pe ON pe.comanda_pesagem_id = cp.id
            INNER JOIN tb_material m ON m.id = pe.material_id
            LEFT JOIN tb_funcionario f1 ON pe.funcionario_pesagem1 = f1.id
            LEFT JOIN tb_funcionario f2 ON pe.funcionario_pesagem2 = f2.id
            WHERE cp.id = @Id
        ";

        var itens = (await db.QueryAsync<ItemDto>(itensSql, new { Id = id })).ToList();

        // Trato o nome dos funcionários (igual lógica do PHP)
        foreach (var item in itens)
        {
            item.FuncionarioPesoBruto = NomeTratado(item.FuncionarioPesoBruto);
            item.FuncionarioTara = NomeTratado(item.FuncionarioTara);
        }

        // Converto a data para string formatada
        DateTime dataInicio = dados.DataInicio;

        return new ComandaDetalhes
        {
            ComandaId = dados.ComandaId,
            Comanda = dados.Comanda,
            ComandaCodigoBarras = dados.ComandaCodigoBarras,
            StatusComanda = dados.StatusComanda,
            Operacao = dados.operacao ?? "",
            Cliente = dados.Cliente ?? "SEM CLIENTE",
            VeiculoPlaca = dados.VeiculoPlaca ?? "--",
            VeiculoDescricao = dados.VeiculoDescricao ?? "--",
            Data = dataInicio.ToString("dd/MM/yyyy"),
            Hora = dataInicio.ToString("HH:mm"),
            Itens = itens ?? new List<ItemDto>()
        };
    }

    // Trata nome para pegar só primeiro e último
    private string NomeTratado(string? nomeCompleto)
    {
        if (string.IsNullOrWhiteSpace(nomeCompleto))
            return "";

        var partes = nomeCompleto
            .Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (partes.Length == 1)
            return partes[0];

        return $"{partes.First()} {partes.Last()}";
    }

    public async Task<bool> AtualizarComanda(ComandaUpdateDto dto)
    {
        using var db = Connection();

        // Atualizo os dados principais da comanda
        var sql = @" 
            UPDATE tb_comanda_pesagem
            SET 
                operacao = @Operacao,
                veiculo_placa = @VeiculoPlaca,
                veiculo_descricao = @VeiculoDescricao,
                cliente_id = @ClienteId,
                observacoes = @Observacoes
            WHERE id = @ComandaPesagemId
        ";

        var linhas = await db.ExecuteAsync(sql, dto);

        // Se afetou alguma linha, deu certo
        return linhas > 0;
    }

    public async Task<bool> Cancelar(int comandaPesagemId)
    {
        using var db = Connection();

        // Atualizo o status para 2 (cancelada/finalizada)
        var sql = @"
            UPDATE tb_comanda_pesagem
            SET status_id = 2
            WHERE id = @Id
        ";

        var linhas = await db.ExecuteAsync(sql, new
        {
            Id = comandaPesagemId
        });

        return linhas > 0;
    }

    public async Task<bool> DeleteItem(int itemId)
    {
        using var db = Connection();

        // Remove o item da tabela de pesagem
        var sql = @"DELETE FROM tb_pesagem WHERE id = @Id";

        var linhas = await db.ExecuteAsync(sql, new
        {
            Id = itemId
        });

        return linhas > 0;
    }
}