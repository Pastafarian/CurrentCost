using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace CurrentCost.Infrastructure
{
    /// <summary>
    /// Abstract handler for all exceptions.
    /// </summary>
    public class ExceptionHandlerMiddleware
    {
        // Enrich is a custom extension method that enriches the Serilog functionality - you may ignore it
        private static readonly ILogger Logger = Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);

        private readonly RequestDelegate _next;

        public ExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                // log the error
                Logger.Error(exception, "error during executing {Context}", context.Request.Path.Value);
                var response = context.Response;
                response.ContentType = "application/json";

                // get the response code and message
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await response.WriteAsync("Error");
            }
        }
    }
}
