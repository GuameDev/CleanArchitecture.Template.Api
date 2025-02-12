
using CleanArchitecture.Template.Api.Extensions;
using CleanArchitecture.Template.Application;
using CleanArchitecture.Template.Host.Extensions;
using CleanArchitecture.Template.Infrastructure;
using FluentValidation;
using Serilog;

namespace CleanArchitecture.Template.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));

            builder.Services
                      .AddSettings(builder.Configuration)
                      .AddApiServices(builder.Configuration)
                      .AddApplicationServices()
                      .AddInfrastructureServices();

            builder.Services.AddCustomHealthChecks();

            var app = builder.Build();

            app.MapStaticAssets();

            app.UseSerilogRequestLogging(options => options.IncludeQueryInRequestPath = true);

            app.UseHealthChecksEndpoints();

            app.Configure();

            app.Run();
        }
    }
}
