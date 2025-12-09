using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Assessment_M6.Application.Interfaces;
using Assessment_M6.Application.Services;
using Assessment_M6.Domain.Interfaces;
using Assessment_M6.Infrastructure.Data;
using Assessment_M6.Infrastructure.Repositories;
using Microsoft.OpenApi.Models;
using Assessment_M6.Infrastructure.Extensions;
using Assessment_M6.Infrastructure.Repositories;
using EduTrack.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// ===================== Conexion a la DB =====================
//Se obtiene la cadena de conexion desde el appsetings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// ===================== Inyeccion de dependencias =====================
// Repositorios
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Servicios Application
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IUserService, UserService>();

// Servicios de utilidad
builder.Services.AddScoped<TokenService>();

// ===================== Configuracion JWT =====================
//Configura el sistema de autenticación para validar tokens JWT en las solicitudes HTTP.
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });

// ===================== Controladores y Swagger =====================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Inmobiliaria API",
        Version = "v1",
        Description = "API para la administracion y gestion de una empresa inmoviliaria"
    });


    // Configuración del botón Authorize en Swagger(JWT)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduce 'Bearer' deja un espacio y pon el {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});



// ===================== Configuracion del Cors (por si me da tiempo de hacer Front) =====================
var corsPolicyName = "AllowSpecificOrigins";

builder.Services.AddCors( options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod(); // si el front envía cookies o auth headers
        });
});


// ===================== Construcion y pipeline =====================
var app = builder.Build();

// Testiar la conexion a la db para tener control de posibles errores
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.OpenConnection();
        Console.WriteLine("Conexion a la base de datos exitosa.");
        db.Database.CloseConnection();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error en la conexion: {ex.Message}");
    }
}

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Local")
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Administracion Inmobiliaria API v1");
        c.RoutePrefix = string.Empty;
    });
}
//Ejecutar este comando para poder hacer pruebas locales si hay algun problema:
//export ASPNETCORE_ENVIRONMENT=Local dotnet run --project ProductCatalog.Api

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();