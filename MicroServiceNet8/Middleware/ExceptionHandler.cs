using MicroServiceNet8.DTO.Common;
using MicroServiceNet8.Helper;
using System.Net;
using System.ServiceModel;

namespace AuthenNet8.Middleware
{
    public class ExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandler> _logger;

        public ExceptionHandler(RequestDelegate next, ILogger<ExceptionHandler> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            // Log lỗi và trả về lỗi gốc
            catch (FaultException<DTOError> ex)
            {
                _logger.LogError(ex.StackTrace);
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await HandleResponseAsync(context, ex);
            }
            // Trả về HTTP 401
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex.StackTrace);
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                FaultException<DTOError> faultException = HelperFault.ServiceFault(ex);
                await HandleResponseAsync(context, faultException);
            }
            // EF Core
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                _logger.LogError(ex.StackTrace);
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                #if (DEBUG)
                var msg = $"{ex.Message}\r\nDetail: {ex.InnerException.Message}\r\nEntities:\r\n {string.Join("-", ex.Entries.Select(e => e.ToString() + "\r\n"))}";
                #else
                var msg = ex.Message;
                #endif
                FaultException<DTOError> faultException = HelperFault.ServiceFault(new Exception(msg));
                await HandleResponseAsync(context, faultException);
            }
            // Others
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                FaultException<DTOError> faultException = HelperFault.ServiceFault(ex);
                await HandleResponseAsync(context, faultException);
            }
        }

        private async Task HandleResponseAsync(HttpContext context, FaultException<DTOError> exception)
        {
            _logger.LogError(exception.Message);
            await context.Response.WriteAsJsonAsync(exception.Detail, options: new System.Text.Json.JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                IgnoreNullValues = false
            });
        }
    }
}
