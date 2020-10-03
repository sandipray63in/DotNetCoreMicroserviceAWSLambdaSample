using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RestfulWebAPINetCore
{
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public AuditMiddleware(RequestDelegate next, ILogger logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            httpContext.Request.EnableBuffering();
            // Leave the body open so the next middleware can read it.
            using (var reader = new StreamReader(
                httpContext.Request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true))
            {
                var requestBody = await reader.ReadToEndAsync();
                //TODO - Should be logged to database may be via an asynchronous microservice
                if (requestBody.IsNotNullOrEmpty())
                {
                    _logger.LogInformation(requestBody);
                }
                // Reset the request body stream position so the next middleware can read it
                httpContext.Request.Body.Position = 0;
            }

            var origResponseBodyStream = httpContext.Response.Body;
            var responseBodyMemStream = new MemoryStream();
            httpContext.Response.Body = responseBodyMemStream;

            await _next.Invoke(httpContext);

            responseBodyMemStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(responseBodyMemStream).ReadToEndAsync();
            //TODO - Should be logged to database may be via an asynchronous microservice
            if (responseBody.IsNotNullOrEmpty())
            {
                _logger.LogInformation(responseBody);
            }
            responseBodyMemStream.Seek(0, SeekOrigin.Begin);
            await responseBodyMemStream.CopyToAsync(origResponseBodyStream);
            httpContext.Response.Body = origResponseBodyStream;
        }
    }
}

