using System;
using ACERRA_WEB_API.Data;
using ACERRA_WEB_API.Models;

namespace ACERRA_WEB_API.Endpoints;

public static class ComandaEndpoint
{
    public static void MapComandaEndpoints(this WebApplication app)
    {
        // Crio um grupo de rotas para organizar tudo relacionado a "comanda"
        var group = app.MapGroup("/comanda");

        // Buscar comanda pelo código de barras e setor
        group.MapGet("/", async (string codigo_barras, string setor, ComandaRepository repo) =>
        {
            try
            {
                // Busco a comanda no repositório
                var comanda = await repo.GetComanda(codigo_barras, setor);

                // Se não encontrar, retorno 404
                if (comanda is null)
                {
                    return Results.NotFound(new
                    {
                        success = false,
                        mensagem = "Comanda não encontrada",
                    });
                }

                // Se encontrou, retorno sucesso com os dados
                return Results.Ok(new
                {
                    success = true,
                    mensagem = "Comanda encontrada",
                    dados = comanda
                });
            }
            catch (Exception)
            {
                // Erro genérico
                return Results.Problem("Erro interno ao buscar comanda");
            }
        });

        // Listar comandas por setor
        group.MapGet("/lista", async (string setor, ComandaRepository repo) =>
        {
            try
            {
                // Valido se o setor foi informado
                if (string.IsNullOrWhiteSpace(setor))
                {
                    return Results.Ok(new
                    {
                        success = false,
                        mensagem = "Nenhum setor foi informado"
                    });
                }

                // Busco as comandas
                var comandas = await repo.GetComandas(setor);

                // Retorno a lista (mesmo vazia)
                return Results.Ok(new
                {
                    success = true,
                    mensagem = comandas.Any()
                        ? "Comandas encontradas"
                        : "Nenhuma comanda encontrada",
                    dados = comandas
                });
            }
            catch (Exception)
            {
                return Results.Problem("Erro interno ao buscar comandas");
            }
        });

        // Buscar detalhes da comanda por ID
        group.MapGet("/detalhes/{id}", async (int id, ComandaRepository repo) =>
        {
            try
            {
                // Valido o ID
                if (id <= 0)
                {
                    return Results.BadRequest(new
                    {
                        success = false,
                        mensagem = "ID inválido"
                    });
                }

                // Busco os detalhes
                var detalhes = await repo.GetDetalhes(id);

                // Se não encontrar
                if (detalhes is null)
                {
                    return Results.NotFound(new
                    {
                        success = false,
                        mensagem = "Comanda não encontrada"
                    });
                }

                // Retorno os dados
                return Results.Ok(new
                {
                    success = true,
                    mensagem = "Comandas encontradas",
                    dados = detalhes
                });
            }
            catch (Exception)
            {
                return Results.Problem("Erro interno ao buscar detalhes");
            }
        });

        // Criar nova comanda
        group.MapPost("/", async (ComandaCreateDto dto, ComandaRepository repo) =>
        {
            try
            {
                // Regra de negócio: não pode existir comanda aberta com o mesmo ID
                var existeAberta = await repo.ExisteComandaAberta(dto.Id);

                if (existeAberta)
                {
                    return Results.BadRequest(new
                    {
                        success = false,
                        mensagem = "Essa comanda já está aberta",
                        dados = false
                    });
                }

                // Crio a comanda
                var id = await repo.Create(dto);

                return Results.Ok(new
                {
                    success = true,
                    mensagem = "Comanda cadastrada com sucesso!",
                    dados = true
                });
            }
            catch (Exception)
            {
                return Results.Problem("Erro interno ao cadastrar comanda");
            }
        });

        // Cancelar ou finalizar comanda
        group.MapPut("/cancelar/{id}", async (int id, ComandaRepository repo) =>
        {
            try
            {
                // Faço o update
                var sucesso = await repo.Cancelar(id);

                // Se não encontrou ou já estava finalizada
                if (!sucesso)
                {
                    return Results.NotFound(new
                    {
                        success = false,
                        mensagem = "Comanda não encontrada ou já finalizada",
                        dados = false
                    });
                }

                return Results.Ok(new
                {
                    success = true,
                    mensagem = "Comanda finalizada com sucesso!",
                    dados = true
                });
            }
            catch (Exception)
            {
                return Results.Problem("Erro interno ao atualizar comanda");
            }
        });

        // Atualizar comanda
        group.MapPut("/", async (ComandaUpdateDto dto, ComandaRepository repo) =>
        {
            try
            {
                // Atualizo a comanda
                var atualizado = await repo.AtualizarComanda(dto);

                if (!atualizado)
                {
                    return Results.Ok(new
                    {
                        success = false,
                        mensagem = "Comanda nao encontrada"
                    });
                }

                return Results.Ok(new
                {
                    success = true,
                    mensagem = "Comanda atualizada com sucesso!"
                });
            }
            catch (Exception ex)
            {
                // Retorno o erro para debug
                return Results.Ok(new
                {
                    success = false,
                    mensagem = "Erro ao atualizar comanda: " + ex.Message
                });
            }
        });

        // Deletar item da comanda
        group.MapDelete("detalhes/delete_item/{id}", async (int id, ComandaRepository repo) =>
        {
            try
            {
                // Valido o ID
                if (id <= 0)
                {
                    return Results.BadRequest(new
                    {
                        success = false,
                        mensagem = "ID inválido"
                    });
                }

                // Removo o item
                var sucesso = await repo.DeleteItem(id);

                if (!sucesso)
                {
                    return Results.NotFound(new
                    {
                        success = false,
                        mensagem = "Não foi possível remover o item",
                        dados = false
                    });
                }

                return Results.Ok(new
                {
                    success = true,
                    mensagem = "Item removido com sucesso!",
                    dados = true
                });
            }
            catch (Exception)
            {
                return Results.Problem("Erro interno ao remover item");
            }
        });
    }
}