using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using api_aggregations.Models;
using api_aggregations.Filters;
using api_aggregations.Services;
using api_aggregations.Controllers;
using api_aggregations.Data;


var builder = WebApplication.CreateBuilder(args);

Env.Load();

var connString = Environment.GetEnvironmentVariable("SECRET")?.Trim().Trim('"');
if (string.IsNullOrWhiteSpace(connString))
{
    throw new InvalidOperationException("Missing SQL Server connection string. Set env var SECRET (or provide it via .env).");
}

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiExceptionFilter>();
});
builder.Services.AddValidation();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSqlServer<AppDbContext>(connString);

builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

builder.Services.AddScoped<ProdutoReservadoService>();
builder.Services.AddScoped<ReservaService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();



