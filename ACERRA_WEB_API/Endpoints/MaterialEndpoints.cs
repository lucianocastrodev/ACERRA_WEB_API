using ACERRA_WEB_API.Data;
using ACERRA_WEB_API.Models;
using Microsoft.AspNetCore.SignalR;
using MySql.Data.MySqlClient;

namespace ACERRA_WEB_API.Endpoints;

public static class MaterialEndpoints
{
    public static void MapProdutoEndpoints(this WebApplication app)
    {


        var group = app.MapGroup("/material");

        // GET /material
        group.MapGet("/", async (MaterialRepository repo) =>
        {
            var produtos = await repo.GetAll();
            return Results.Ok(produtos);
        });

        // GET /material/{id}
        group.MapGet("/{id}", async (int id, MaterialRepository repo) =>
        {
            var material = await repo.GetById(id);

            return material is not null
                ? Results.Ok(material)
                : Results.NotFound(new { mensagem = "material não encontrado" });
        });

        // POST /material
        group.MapPost("/", async (Material material, MaterialRepository repo) =>
        {
            try
            {
                var id = await repo.Create(material);
                material.Id = id;

                return Results.Created($"/material/{id}", new
                {
                    success = true,
                    mensagem = "material cadastrado com sucesso",
                    dados = material
                });

            }
            catch (MySqlException ex) when (ex.SqlState == "23505")
            {
                return Results.BadRequest(new
                {
                    erro = "codigo_barras_duplicado",
                    mensagem = "Já existe um material com este código de barras"
                });
            }
            catch (Exception)
            {
                return Results.Problem("Erro interno ao criar material");
            }
        });

        // PUT /material/{id}
        group.MapPut("/{id}", async (int id, Material material, MaterialRepository repo) =>
        {
            try
            {
                material.Id = id;

                var updated = await repo.Update(material);

                if (!updated)
                {
                    return Results.NotFound(new { mensagem = "material não encontrado" });
                }

                return Results.Ok(new
                {
                    success = true,
                    mensagem = "material atualizado com sucesso",
                    dados = material
                });
            }
            catch (MySqlException ex) when (ex.SqlState == "23505")
            {
                return Results.BadRequest(new
                {
                    erro = "codigo_barras_duplicado",
                    mensagem = "Já existe um material com este código de barras"
                });
            }
            catch (Exception)
            {
                return Results.Problem("Erro interno ao atualizar material");
            }
        });

        // DELETE /material/{id}
        group.MapDelete("/{id}", async (int id, MaterialRepository repo) =>
        {
            try
            {
                var deleted = await repo.Delete(id);
                return deleted
                    ? Results.NoContent()
                    : Results.NotFound(new { mensagem = "material não encontrado" });
            }
            catch (Exception)
            {
                return Results.Problem("Erro interno ao deletar material");
            }
        });
    }
}