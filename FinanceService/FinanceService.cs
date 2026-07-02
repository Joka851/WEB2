using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using FinanceService.Data;

namespace FinanceService
{
    internal sealed class FinanceService : StatelessService
    {
        public FinanceService(StatelessServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        var currentDirectory = Directory.GetCurrentDirectory();

                        var configuration = new ConfigurationBuilder()
                            .SetBasePath(currentDirectory)
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .AddEnvironmentVariables()
                            .Build();

                        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
                        {
                            ContentRootPath = currentDirectory
                        });

                        builder.Services.AddSingleton<StatelessServiceContext>(serviceContext);
                        builder.Services.AddSingleton<IConfiguration>(configuration);

                        var endpoint = serviceContext.CodePackageActivationContext.GetEndpoint("ServiceEndpoint");

                        builder.WebHost
                            .UseKestrel(options =>
                            {
                                options.ListenAnyIP(endpoint.Port);
                            })
                            .UseContentRoot(currentDirectory);

                        // ---- DbContext ----
                        builder.Services.AddDbContext<FinanceDbContext>(options =>
                            options.UseSqlServer(configuration.GetConnectionString("FinanceDB")));

                        // ---- JWT Authentication ----
                        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                            .AddJwtBearer(options =>
                            {
                                options.TokenValidationParameters = new TokenValidationParameters
                                {
                                    ValidateIssuer = true,
                                    ValidateAudience = true,
                                    ValidateLifetime = true,
                                    ValidateIssuerSigningKey = true,
                                    ValidIssuer = configuration["Jwt:Issuer"],
                                    ValidAudience = configuration["Jwt:Audience"],
                                    IssuerSigningKey = new SymmetricSecurityKey(
                                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
                                };
                            });

                        builder.Services.AddAuthorization();

                        // ---- HttpClient za komunikaciju sa TravelService (provera vlasništva nad planom) ----
                        builder.Services.AddHttpClient("TravelService", client =>
                        {
                            var travelUrl = configuration["ServiceUrls:TravelService"];
                            if (!string.IsNullOrEmpty(travelUrl))
                            {
                                client.BaseAddress = new Uri(travelUrl);
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

                        // ---- Kreiraj bazu ako ne postoji ----
                        using (var scope = app.Services.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
                            try
                            {
                                dbContext.Database.EnsureCreated();
                                ServiceEventSource.Current.ServiceMessage(serviceContext, "FinanceDB ensured created successfully");
                            }
                            catch (Exception ex)
                            {
                                ServiceEventSource.Current.ServiceMessage(serviceContext, $"FinanceDB error: {ex.Message}");
                                throw;
                            }
                        }

                        ServiceEventSource.Current.ServiceMessage(serviceContext, "FinanceService Kestrel started successfully");
                        return app;
                    }))
            };
        }
    }
}