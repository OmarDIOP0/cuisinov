using System.Net;

namespace CantineBack.Helpers
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                var response = context.Response;
                //response.ContentType = "application/json";

                switch (error)
                {
                    case ApplicationException e:
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        _logger.LogError(e.Message);
                        break;
                    case KeyNotFoundException e:
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        _logger.LogError(e.Message);
                        break;
                    default:
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;

                        try
                        {
                            var innerExcep = error.InnerException ?? null;

                            if (innerExcep != null)
                            {
                                _logger.LogError($"{error.Message} -- Inner Exception {error.InnerException} -- Stack Trace {error.StackTrace}");
                            }
                            else
                            {
                                _logger.LogError($"{error.Message} -- Stack Trace {error.StackTrace}");
                            }
                        }
                        catch (NullReferenceException)
                        {
                            _logger.LogError($"{error.Message} -- Stack Trace {error.StackTrace}");
                        }

                        break;
                }

                //var result = JsonSerializer.Serialize(new { message = error?.Message });
                //await response.WriteAsync(result);
                //throw;
            }
        }
    }
}
