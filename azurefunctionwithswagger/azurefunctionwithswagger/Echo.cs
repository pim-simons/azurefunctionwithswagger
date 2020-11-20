using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Serilog;
using azurefunctionwithswagger;
using azurefunctionwithswagger.models;
using Newtonsoft.Json;
using GuardNet;
using System.Diagnostics;
using System.Web.Http;
using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Attributes;
using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Enums;
using System.Net;

[assembly: FunctionsStartup(typeof(Startup))]
namespace azurefunctionwithswagger
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var instrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY", EnvironmentVariableTarget.Process);

            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithComponentName("azurefunctionwithswagger")
                .Enrich.WithVersion()
                .WriteTo.Console(Serilog.Events.LogEventLevel.Warning)
                //.WriteTo.AzureApplicationInsights(instrumentationKey, Serilog.Events.LogEventLevel.Warning)
                .CreateLogger();

            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProvidersExceptFunctionProviders();
                loggingBuilder.AddSerilog(logger);
            });
        }
    }

    public class Echo
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<Echo> _logger;

        public Echo(IConfiguration configuration, ILogger<Echo> logger)
        {
            Guard.NotNull(configuration, nameof(configuration));
            Guard.NotNull(logger, nameof(logger));

            _configuration = configuration;
            _logger = logger;
        }

        [FunctionName("Echo")]
        [OpenApiOperation(operationId: "Echo", tags: new[] { "" }, Summary = "Returns the input value in the output", Description = "Returns the input value in output", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(Request), Required = true, Description = "The request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Response), Summary = "This contains the result", Description = "The response")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            var stopWatch = Stopwatch.StartNew();

            try
            {
                string message = await new StreamReader(req.Body).ReadToEndAsync();
                Request request = JsonConvert.DeserializeObject<Request>(message);

                Response response = new Response()
                {
                    Output = request.Input
                };
                return (ActionResult)new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred: " + ex.ToString());
                req.HttpContext.Response.StatusCode = 500;
                return (ActionResult)new ExceptionResult(ex, true);
            }
            finally
            {
                _logger.LogRequest(req.HttpContext.Request, req.HttpContext.Response, stopWatch.Elapsed);
            }
        }
    }
}
