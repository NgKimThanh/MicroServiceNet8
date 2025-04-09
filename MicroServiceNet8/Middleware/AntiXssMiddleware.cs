using MicroServiceNet8.API.Helper.Middleware;
using Microsoft.IO;
using System.Net;

namespace AuthenNet8.API.Middleware
{
    public class AntiXssMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AntiXssMiddleware> _logger;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;

        public AntiXssMiddleware(RequestDelegate next, ILogger<AntiXssMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        }

        public async Task Invoke(HttpContext context)
        {
            await ValidateXssAttack(context);
            await _next.Invoke(context);
        }

        private static string ReadStreamInChunks(Stream stream)
        {
            const int readChunkBufferLength = 4096;
            stream.Seek(0, SeekOrigin.Begin);
            using var textWriter = new StringWriter();
            using var reader = new StreamReader(stream);
            var readChunk = new char[readChunkBufferLength];
            int readChunkLength;
            do
            {
                readChunkLength = reader.ReadBlock(readChunk,
                                                   0,
                                                   readChunkBufferLength);
                textWriter.Write(readChunk, 0, readChunkLength);
            } while (readChunkLength > 0);
            return textWriter.ToString();
        }

        private async Task ValidateXssAttack(HttpContext context)
        {
            // Check XSS in URL
            if (!string.IsNullOrWhiteSpace(context.Request.Path.Value))
            {
                var url = context.Request.Path.Value;

                if (HelperCrossSiteScriptingValidation.IsDangerousString(url, out string label))
                {
                    throw new System.Exception($"Url có chứa mã thực thi. Mã lỗi: {label}");
                }
            }

            // Check XSS in query string
            if (!string.IsNullOrWhiteSpace(context.Request.QueryString.Value))
            {
                var queryString = WebUtility.UrlDecode(context.Request.QueryString.Value);

                if (HelperCrossSiteScriptingValidation.IsDangerousString(queryString, out string label))
                {
                    throw new System.Exception($"Query string có chứa mã thực thi. Mã lỗi: {label}");
                }
            }

            if (context.Request.Body.CanRead && !context.Request.HasFormContentType)
            {
                context.Request.EnableBuffering();
                await using var requestStream = _recyclableMemoryStreamManager.GetStream();
                await context.Request.Body.CopyToAsync(requestStream);
                string content = ReadStreamInChunks(requestStream);
                context.Request.Body.Position = 0;
                if (!string.IsNullOrWhiteSpace(content))
                {
                    if (HelperCrossSiteScriptingValidation.IsDangerousString(content, out string label))
                    {
                        throw new System.Exception($"Body có chứa mã thực thi. Mã lỗi: '{label}'");
                    }
                }
            }
        }
    }
}
