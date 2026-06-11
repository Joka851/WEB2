using Microsoft.EntityFrameworkCore;
using TravelService.Data;

var builder = WebApplication.CreateBuilder(args);

// ***** DODATO - PORT 5004 *****
builder.WebHost.UseUrls("http://localhost:5004");
// *****************************

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TravelDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TravelDB")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();