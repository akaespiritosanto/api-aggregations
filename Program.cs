using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using api_aggregations.Models;
using api_aggregations.Filters;
using api_aggregations.Services;
using api_aggregations.Controllers;
using api_aggregations.Data;
using Microsoft.OpenApi.Models;


Env.Load();

var builder = WebApplication.CreateBuilder(args);

var connString = Environment.GetEnvironmentVariable("SECRET")?.Trim().Trim('"');
if (string.IsNullOrWhiteSpace(connString))
{
    throw new InvalidOperationException("Missing SQL Server connection string. Set env var SECRET (or provide it via .env).");
}

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiExceptionFilter>();
    options.Filters.AddService<ApiKeyAuthFilter>();
    options.Filters.Add<RemoveZeroArrayItemsFilter>();
});
builder.Services.AddValidation();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSqlServer<AppDbContext>(connString);
builder.Services.AddCors();

builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API key needed to access the endpoints. Use header: X-API-KEY: {your key}",
        In = ParameterLocation.Header,
        Name = "X-API-KEY",
        Type = SecuritySchemeType.ApiKey
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            new List<string>()
        }
    });
});

builder.Services.AddScoped<ProdutoReservadoService>();
builder.Services.AddScoped<ReservaService>();
builder.Services.AddScoped<RelatorioValoresEDuracaoReservasService>();
builder.Services.AddSingleton<ApiKeyAuthFilter>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var corsOrigin = builder.Configuration.GetValue("CORS-origin", "*") ?? "*";
var corsOrigins = corsOrigin.Replace(" ", "").Split(",", StringSplitOptions.RemoveEmptyEntries);

app.UseCors(policy =>
{
    if (corsOrigins.Length == 1 && corsOrigins[0] == "*")
    {
        policy.AllowAnyOrigin();
    }
    else
    {
        policy.WithOrigins(corsOrigins);
    }

    policy
        .AllowAnyMethod()
        .AllowAnyHeader()
        .WithExposedHeaders("X-LANGUAGE");
});

app.UseAuthorization();
app.MapControllers();

app.Run();



