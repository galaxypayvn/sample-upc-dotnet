using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using IGeekFan.AspNetCore.RapiDoc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Filters;

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
        
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1",new OpenApiInfo{Title = "API V1",Version = "v1"});
            //var filePath = Path.Combine(System.AppContext.BaseDirectory,$"{typeof(Startup).Assembly.GetName().Name}.xml");
            //c.IncludeXmlComments(filePath, true);
        });

        SelfLog.Enable(Log.Error);
        LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithExceptionDetails()
            .ReadFrom.Configuration(Configuration)
            .WriteTo.Console();
        
        List<string> excludes = new List<string>
        {
            "Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker",
            "Microsoft.AspNetCore.Mvc.Infrastructure.ObjectResultExecutor",
            "Microsoft.AspNetCore.Routing.EndpointMiddleware",
            "Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware"
        };
        foreach (string source in excludes)
        {
            loggerConfiguration.Filter.ByExcluding(Matching.FromSource(source));
        }

        Log.Logger = loggerConfiguration.CreateLogger();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();

            app.UseRapiDocUI(c =>
            {
                c.RoutePrefix = "2345234"; // serve the UI at root
                c.SwaggerEndpoint("/v1/api-docs", "V1 Docs");
                c.GenericRapiConfig = new GenericRapiConfig()
                {
                    RenderStyle="focused",
                    Theme="light",//light,dark,focused   
                };
            });
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
            endpoints.MapSwagger("{documentName}/api-docs");
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