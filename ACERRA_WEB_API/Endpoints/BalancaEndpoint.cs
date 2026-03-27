using ACERRA_WEB_API.Data;

namespace ACERRA_WEB_API.Endpoints
{
    public static class BalancaEndpoint
    {
        public static void MapbalancaEndpint(this WebApplication app)
        {
            var group = app.MapGroup("/balancas");

            // GET /balanca/{setor}
            group.MapGet("/{setor}", async (string setor, BalancaRepository repo) =>
            {
                var balancas = await repo.GetAll(setor);

                return balancas is not null
                    ? Results.Ok(balancas)
                    : Results.NotFound(new { mensagem = "Balanças não encontradas" });
            });
        }
    }
}
