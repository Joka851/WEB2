using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using QuestPDF.Infrastructure;
using TravelService.Data;

namespace TravelService
{
    internal sealed class TravelService : StatefulService
    {
        public TravelService(StatefulServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new ServiceReplicaListener[]
            {
                new ServiceReplicaListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        // QuestPDF zahteva eksplicitno postavljanje licence pre generisanja bilo kog PDF-a.
                        // Community licenca je besplatna za ovakve (nekomercijalne/edukativne) projekte.
                        QuestPDF.Settings.License = LicenseType.Community;

                        var currentDirectory = Directory.GetCurrentDirectory();

                        var configuration = new ConfigurationBuilder()
                            .SetBasePath(currentDirectory)
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .AddEnvironmentVariables()
                            .Build();

                        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
                        {
                            ContentRootPath = currentDirectory,
                            WebRootPath = Path.Combine(currentDirectory, "wwwroot")
                        });

                        builder.Services.AddSingleton<StatefulServiceContext>(serviceContext);
                        builder.Services.AddSingleton<IConfiguration>(configuration);

                        // ---- Endpoint port ----
                        var endpoint = serviceContext.CodePackageActivationContext.GetEndpoint("ServiceEndpoint");

                        builder.WebHost
                            .UseKestrel(options =>
                            {
                                options.ListenAnyIP(endpoint.Port);
                            })
                            .UseContentRoot(currentDirectory);

                        // ---- DbContext ----
                        var connectionString = configuration.GetConnectionString("TravelDB");
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Connection string: {connectionString}");

                        builder.Services.AddDbContext<TravelDbContext>(options =>
                            options.UseSqlServer(connectionString));

                        // ---- JWT Authentication ----
                        var jwtKey = configuration["Jwt:Key"];
                        var jwtIssuer = configuration["Jwt:Issuer"];
                        var jwtAudience = configuration["Jwt:Audience"];

                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"JWT Issuer: {jwtIssuer}");

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
                                    IssuerSigningKey = new SymmetricSecurityKey(
                                        Encoding.UTF8.GetBytes(jwtKey ?? "TravelPlannerSuperSecretKey123!@#"))
                                };
                            });

                        builder.Services.AddAuthorization();

                        // ---- HttpClient za komunikaciju sa FinanceService (cascade delete) ----
                        builder.Services.AddHttpClient("FinanceService", client =>
                        {
                            var financeUrl = configuration["ServiceUrls:FinanceService"];
                            if (!string.IsNullOrEmpty(financeUrl))
                            {
                                client.BaseAddress = new Uri(financeUrl);
                            }
                        });

                        // ---- CORS ----
                        builder.Services.AddCors(options =>
                        {
                            options.AddPolicy("AllowAll", policy =>
                            {
                                policy.AllowAnyOrigin()
                                      .AllowAnyMethod()
                                      .AllowAnyHeader();
                            });
                        });

                        builder.Services.AddControllers();
                        builder.Services.AddEndpointsApiExplorer();
                        builder.Services.AddSwaggerGen();

                        var app = builder.Build();

                        if (app.Environment.IsDevelopment())
                        {
                            app.UseSwagger();
                            app.UseSwaggerUI();
                        }

                        app.UseCors("AllowAll");
                        app.UseAuthentication();
                        app.UseAuthorization();
                        app.MapControllers();

                        // Kreiraj bazu ako ne postoji
                        using (var scope = app.Services.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<TravelDbContext>();
                            try
                            {
                                dbContext.Database.EnsureCreated();
                                ServiceEventSource.Current.ServiceMessage(serviceContext, "Database ensured created successfully");
                            }
                            catch (Exception ex)
                            {
                                ServiceEventSource.Current.ServiceMessage(serviceContext, $"Database error: {ex.Message}");
                                throw;
                            }
                        }

                        ServiceEventSource.Current.ServiceMessage(serviceContext, "Kestrel started successfully");
                        return app;
                    }))
            };
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            ServiceEventSource.Current.ServiceMessage(Context, "!!! TRAVEL SERVICE RunAsync STARTED !!!");

            int counter = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ServiceEventSource.Current.ServiceMessage(Context, $"TravelService is running... Count: {++counter}");

                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            }
        }
    }
}