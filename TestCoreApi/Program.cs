using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using TestCoreApi.Data;
using TestCoreApi.Repositories;
using TestCoreApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Repositories & Services ───────────────────────────────────────────────────
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService,    ProductService>();
builder.Services.AddScoped<IJwtService,        JwtService>();
builder.Services.AddScoped<IAuthService,       AuthService>();

// ── JWT Authentication ────────────────────────────────────────────────────────
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
        ClockSkew                = TimeSpan.Zero
    };

    // Return proper 401 JSON instead of empty response
    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            context.HandleResponse();
            context.Response.StatusCode  = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                """{"message":"Unauthorized. Please login and provide a valid Bearer token."}""");
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

// ── Controllers + Swagger ─────────────────────────────────────────────────────
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
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Paste your JWT token here. 'Bearer ' prefix is added automatically."
    });

    c.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference(schemeName), new List<string>() }
    });
});

var app = builder.Build();

// ── Ensure DB + tables exist (safe — only creates if missing) ─────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();   // creates DB + tables if they don't exist; no-op if already there
}

// ── Global exception handler ──────────────────────────────────────────────────
app.UseExceptionHandler(errApp => errApp.Run(async context =>
{
    context.Response.StatusCode  = 500;
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync("""{"message":"An unexpected error occurred. Please try again later."}""");
}));

// ── Middleware pipeline ───────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TestCoreApi v1"));
}

app.UseHttpsRedirection();
app.UseAuthentication();   // must be before UseAuthorization
app.UseAuthorization();
app.MapControllers();

app.Run();
