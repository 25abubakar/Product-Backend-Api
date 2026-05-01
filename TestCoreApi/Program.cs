using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TestCoreApi.Data;
using TestCoreApi.Repositories;
using TestCoreApi.Services;
using TestCoreApi.Swagger;

// Disable JWT claim type remapping so ClaimTypes.Role is preserved as-is
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService,    ProductService>();
builder.Services.AddScoped<IJwtService,        JwtService>();
builder.Services.AddScoped<IAuthService,       AuthService>();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey   = jwtSettings["SecretKey"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtSettings["Issuer"],
        ValidAudience            = jwtSettings["Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew                = TimeSpan.Zero,
        NameClaimType = ClaimTypes.Name,   // ? ADD THIS
        RoleClaimType = ClaimTypes.Role    // ? ADD THIS
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            // Exact failure reason console mein print hoga
            Console.WriteLine($"[JWT FAILED] {context.Exception.GetType().Name}: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine($"[JWT OK] Token validated for: {context.Principal?.Identity?.Name}");
            return Task.CompletedTask;
        },
        OnChallenge = async context =>
        {
            context.HandleResponse();
            context.Response.StatusCode  = 401;
            context.Response.ContentType = "application/json";
            var reason = context.AuthenticateFailure?.Message ?? "No token provided";
            await context.Response.WriteAsync(
                $$$"""{"message":"Unauthorized. Please login and provide a valid Bearer token.","reason":"{{{reason}}}"}""");
        },
        OnForbidden = async context =>
        {
            context.Response.StatusCode  = 403;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                """{"message":"Forbidden. You do not have permission to access this resource."}""");
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "TestCoreApi",
        Version     = "v1",
        Description = "Full CRUD API with JWT Authentication"
    });

    const string schemeName = "Bearer";
    c.AddSecurityDefinition(schemeName, new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.ApiKey,   // ApiKey = manual header, always works
        In           = ParameterLocation.Header,
        Description  = "Enter: Bearer {your token}  e.g. Bearer eyJhbG..."
    });

    c.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference(schemeName),
            new List<string>()
        }
    });

    // Fix: automatically attach Bearer to [Authorize] endpoints in Swagger UI
    c.OperationFilter<AuthOperationFilter>();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();   // creates DB + tables if they don't exist; no-op if already there
}

app.UseExceptionHandler(errApp => errApp.Run(async context =>
{
    context.Response.StatusCode  = 500;
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync("""{"message":"An unexpected error occurred. Please try again later."}""");
}));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TestCoreApi v1"));
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
