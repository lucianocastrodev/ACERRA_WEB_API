using ACERRA_WEB_API.Endpoints;
using ACERRA_WEB_API.Data;
using ACERRA_WEB_API.Repository;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<MaterialRepository>();
builder.Services.AddScoped<BalancaRepository>();
builder.Services.AddScoped<ComandaRepository>();
builder.Services.AddScoped<ClienteRepository>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
});

// Validãções
builder.Services.AddValidation();

// CORS — essencial para Blazor WASM + cookies
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.WithOrigins(
            "https://localhost:7153",
            "https://192.168.1.115:7153",
            "http://localhost:5263",
            "http://192.168.1.115:5263",
            "https://bserve.com.br"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials(); // 🔑 necessário para enviar cookies
    });
});

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Registra os endpoints de material
app.MapProdutoEndpoints();
app.MapbalancaEndpint();
app.MapComandaEndpoints();
app.MapClienteEndpoints();

app.Run();
