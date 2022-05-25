using System;
using System.Reflection;
using System.Text.Json;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;
using Serilog.Debugging;
using Serilog.Exceptions;

namespace UPC.DotNet;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    private IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddControllersWithViews()
            .AddJsonOptions(configure =>
            {
                configure.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                configure.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

        services.AddSpaStaticFiles(configuration =>
        {
            configuration.RootPath = "ClientApp/dist";
        });

        // string assemblyPath = AppContext.BaseDirectory + "UPC-Api.dll";
        // Assembly assembly = Assembly.LoadFile(assemblyPath);
        // AssemblyPart part = new(assembly);
        // services.AddControllers().PartManager.ApplicationParts.Add(part);

        SelfLog.Enable(Log.Error);
        LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithExceptionDetails()
            .ReadFrom.Configuration(Configuration)
            .WriteTo.Console();

        Log.Logger = loggerConfiguration.CreateLogger();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        if (!env.IsDevelopment())
        {
            app.UseSpaStaticFiles();
        }

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller}/{action=Index}/{id?}");
        });

        app.UseSpa(spa =>
        {
            spa.Options.SourcePath = "ClientApp";

            if (env.IsDevelopment())
            {
                spa.UseAngularCliServer(npmScript: "start");
            }
        });
    }
}