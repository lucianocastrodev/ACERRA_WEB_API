using ACERRA_WEB_API.Data;
using ACERRA_WEB_API.Repository;

namespace ACERRA_WEB_API.Endpoints;

public static class ClienteEndpoint
{
    public static void MapClienteEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/cliente");
        group.MapGet("/", async (ClienteRepository repo) =>
        {
            try
            {
                var clientes = await repo.GetClientes();

                return Results.Ok(clientes);
            }
            catch (Exception)
            {
                return Results.Problem("Erro ao buscar clientes");
            }
        });
    }
}
