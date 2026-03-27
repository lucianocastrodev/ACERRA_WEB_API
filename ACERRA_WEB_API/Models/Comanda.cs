using System.Text.Json.Serialization;

namespace ACERRA_WEB_API.Models;

// ============================
// 🔵 MODEL PRINCIPAL (BANCO)
// ============================
public class Comanda
{
    public int Id { get; set; }
    public int ComandaPesagemId { get; set; }

    public string Descricao { get; set; } = string.Empty;
    public string Operacao { get; set; } = string.Empty;
    public string Setor { get; set; } = string.Empty;
    public string CodigoBarras { get; set; } = string.Empty;

    public int Status { get; set; }

    public int? ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;

    public string VeiculoPlaca { get; set; } = string.Empty;
    public string VeiculoDescricao { get; set; } = string.Empty;
}

// ============================
// 🟢 CREATE DTO
// ============================
public class ComandaCreateDto
{
    public string Operacao { get; set; } = string.Empty;
    public string Setor { get; set; } = string.Empty;
    public string VeiculoPlaca { get; set; } = string.Empty;
    public string VeiculoDescricao { get; set; } = string.Empty;

    public int ClienteId { get; set; }
    public int Status { get; set; }
    public string Observacoes { get; set; } = string.Empty;

    public int ComandaPesagemId { get; set; }

    public int Id { get; set; }
}

// ============================
// 🟡 UPDATE DTO (IMPORTANTE)
// ============================
public class ComandaUpdateDto
{
    [JsonPropertyName("ComandaPesagemID")] // 🔥 bate com PHP
    public int ComandaPesagemId { get; set; }

    public string Operacao { get; set; } = string.Empty;
    public string VeiculoPlaca { get; set; } = string.Empty;
    public string VeiculoDescricao { get; set; } = string.Empty;

    public int ClienteId { get; set; }
    public string Observacoes { get; set; } = string.Empty;
}

// ============================
// 🔵 DTO DE LISTAGEM
// ============================
public class ComandaDto
{
    public int Id { get; set; }
    public int ComandaPesagemId { get; set; }

    public string Descricao { get; set; } = string.Empty;
    public string Operacao { get; set; } = string.Empty;
    public string CodigoBarras { get; set; } = string.Empty;

    public int Status { get; set; }
}

// ============================
// 🔍 DETALHES COMPLETOS
// ============================
public class ComandaDetalhes
{
    public int ComandaId { get; set; }

    public string Comanda { get; set; } = string.Empty;
    public string ComandaCodigoBarras { get; set; } = string.Empty;

    public int StatusComanda { get; set; }
    public string Operacao { get; set; } = string.Empty;

    public string Cliente { get; set; } = string.Empty;

    public string VeiculoPlaca { get; set; } = string.Empty;
    public string VeiculoDescricao { get; set; } = string.Empty;

    public string Data { get; set; } = string.Empty;
    public string Hora { get; set; } = string.Empty;

    public List<ItemDto> Itens { get; set; } = new();
}

// ============================
// 📦 ITEM DA COMANDA
// ============================
public class ItemDto
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;

    public int StatusId { get; set; }

    public string PesoBruto { get; set; } = string.Empty;
    public string Tara { get; set; } = string.Empty;
    public string Quantidade { get; set; } = string.Empty;

    public string FuncionarioPesoBruto { get; set; } = string.Empty;
    public string FuncionarioTara { get; set; } = string.Empty;
}