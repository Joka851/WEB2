using FinanceService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5003");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ========== DODATO: JWT Autentikacija ==========
var jwtKey = builder.Configuration["Jwt:Key"] ?? "your-super-secret-key-min-32-chars-long!!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TravelPlanner";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "TravelPlannerClient";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();
// ===============================================

builder.Services.AddDbContext<FinanceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FinanceDB")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ========== DODATO: Redoslijed middleware-a je bitan! ==========
app.UseAuthentication();  // OVO MORA BITI PRIJE UseAuthorization
app.UseAuthorization();
// ================================================================

app.MapControllers();
app.Run();