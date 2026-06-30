using Asp.Versioning;
using Directo.Wari.TarifaEngine.API.Common.JsonConverters;
using Directo.Wari.TarifaEngine.API.Extensions;
using Directo.Wari.TarifaEngine.API.Middleware;
using Directo.Wari.TarifaEngine.Application;
using Directo.Wari.TarifaEngine.Application.Common.Interfaces;
using Directo.Wari.TarifaEngine.Application.Common.Options;
using Directo.Wari.TarifaEngine.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using System.Globalization;
using System.Text;
using System.Text.Json;


// ===== PREPARANDO REGION PARA DOCKER ====
var culture = new CultureInfo("es-PE");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("===== ENVIRONMENT =====");
Console.WriteLine(builder.Environment.EnvironmentName);

Console.WriteLine("===== CORS ORIGINS =====");
Console.WriteLine(builder.Configuration["CorsOrigins"]);

// ===== SERILOG =====
builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig.ReadFrom.Configuration(context.Configuration);
});

// ===== CAPAS DE CLEAN ARCHITECTURE =====
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

// ===== CONTROLLERS =====
builder.Services.AddControllers()
    //PERMITIR EL USO DE PASCALCASE MENO IMPACTO PARA EL FRONT ACTUAL
    .AddJsonOptions(option =>
    {
        option.JsonSerializerOptions.Converters.Add(new EmptyStringToNullableIntConverter());
        option.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// ===== API VERSIONING =====
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
    options.AssumeDefaultVersionWhenUnspecified = true;

});

// ===== SWAGGER =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v2", new()
    {
        Title = "WariDirecto-Modulo-TarifaEngine API",
        Version = "v2"
    });

    // ===== BEARER TOKEN EN SWAGGER =====
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",        // minúscula — RFC 7235
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese únicamente el token JWT (sin 'Bearer ')"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

// ===== HTTP CLIENT PARA VALIDACIÓN DE TOKEN =====
var ignoreSslErrors = builder.Configuration.GetValue<bool>("TokenValidation:IgnoreSslErrors", false);
builder.Services.AddHttpClient("TokenValidation")
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        var handler = new HttpClientHandler();
        if (ignoreSslErrors)
            handler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        return handler;
    });

// ===== CORS =====
var corsOrigins = builder.Configuration["CorsOrigins"]?
    .Split(';', StringSplitOptions.RemoveEmptyEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDev", policy =>
    {
        policy.AllowAnyOrigin()
         .AllowAnyHeader()
         .AllowAnyMethod();

        //policy.WithOrigins(corsOrigins!)
        //      .AllowAnyHeader()
        //      .AllowAnyMethod()
        //      .AllowCredentials();
    });
});

// ===== HEALTH CHECKS =====
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!)
    .AddRedis(builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "localhost:6379");

var app = builder.Build();

// ===== MIDDLEWARE PIPELINE =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "WariDirecto API V2");
    });
}

app.UseSerilogRequestLogging();

app.UseRouting();
app.UseCors("AllowDev");

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<BearerTokenValidationMiddleware>();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();