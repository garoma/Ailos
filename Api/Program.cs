using Application.Handlers;
using Application.Requests;
using Application.Validators;
using FluentValidation;
using Infrastructure.Database.CommandStore;
using Infrastructure.Database.QueryStore;
using Infrastructure.Database.Services.Controllers;
using Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Questao5.Infrastructure.Sqlite;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Data;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Token (pode ser movida para appsettings se quiser)
var chaveSecreta = builder.Configuration["Jwt:Key"];
var key = Encoding.ASCII.GetBytes(chaveSecreta);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // true em produção com HTTPS
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false, // ou true se quiser usar
        ValidateAudience = false, // ou true se quiser usar
        ClockSkew = TimeSpan.Zero
    };
});

//builder.Services.AddSwaggerGen(c =>
//{
//    c.OperationFilter<IdempotencyHeaderFilter>();
//    c.SchemaFilter<GuidExampleSchemaFilter>();
//});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddMediatR(Assembly.GetExecutingAssembly());

// sqlite
builder.Services.AddSingleton(new DatabaseConfig { Name = builder.Configuration.GetValue<string>("DatabaseName", "Data Source=database.sqlite") });
builder.Services.AddSingleton<IDatabaseBootstrap, DatabaseBootstrap>();

builder.Services.AddTransient<IContaCorrenteQueryStore, ContaCorrenteQueryStore>();
builder.Services.AddTransient<IMovimentoCommandStore, MovimentoCommandStore>();
builder.Services.AddScoped<IContaCorrenteCommandStore, ContaCorrenteCommandStore>();
builder.Services.AddScoped<IValidator<InativarContaRequest>, InativarContaValidator>();
builder.Services.AddScoped<ITransferenciaCommandStore, TransferenciaCommandStore>();

builder.Services.AddHttpClient("ContaCorrenteApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:7140/"); // ajuste conforme seu ambiente
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddMediatR(typeof(EfetuarTransferenciaHandler).Assembly);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    // Adiciona a definição de segurança
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Insira o token JWT no formato: Bearer {seu token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Aplica a segurança a todos os endpoints
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

builder.Services.AddScoped<ContaCorrenteQueryStore>();
builder.Services.AddScoped<MovimentoCommandStore>();
builder.Services.AddScoped<IContaCorrenteService, ContaCorrenteService>();

builder.Services.AddScoped<IDbConnection>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    return new SqliteConnection(connectionString);
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

#pragma warning disable CS8602 // Dereference of a possibly null reference.
app.Services.GetService<IDatabaseBootstrap>().Setup();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

app.MapAuthEndpoints();
app.MapCriarContaEndpoint();
app.MapMovimentarContaEndpoint();
app.MapConsultarSaldoEndpoint();
app.MapTransferenciaEndpoints();
app.MapInativarContaEndpoints();

app.Run();

// Informações úteis:
// Tipos do Sqlite - https://www.sqlite.org/datatype3.html
