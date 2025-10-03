using System.Net;
using SystemHealthAPI.Exceptions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SystemHealthAPI.Middleware
{
    /// <summary>
    /// Handles all exceptions thrown by the API. Logs the error and returns an appropriate status code.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;

        /// <summary>
        /// Constructor for Class
        /// </summary>
        /// <param name="next"></param>
        /// <param name="logger"></param>
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        /// <summary>
        /// Middleware for invoking necessary exception handling tasks
        /// </summary>
        /// <param name="context">The HTTP context potentially containing status codes</param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (MetricsNotAvailableException e)
            {
                logger.LogWarning(0, e, e.Message);
                await Handle(context, e, HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                logger.LogError(0, ex, "An unexpected exception occurred");
                await Handle(context, ex, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Returns a response containing the ApiErrorResponse errorObject in the body.
        /// </summary>
        /// <param name="context">The request context to set the response on</param>
        /// <param name="e">The Exception caught</param>
        /// <param name="statusCode">The error status code to set on the response</param>
        private static Task Handle(HttpContext context, Exception e, HttpStatusCode statusCode)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            return context.Response.WriteAsync(JsonSerializer.Serialize(e.Message));
        }
    }
}
