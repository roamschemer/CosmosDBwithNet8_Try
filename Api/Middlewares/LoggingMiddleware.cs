using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace Api.Middlewares
{
	public class LoggingMiddleware : IFunctionsWorkerMiddleware
	{
		private readonly ILogger<LoggingMiddleware> _logger;

		public LoggingMiddleware(ILogger<LoggingMiddleware> logger) {
			_logger = logger;
		}

		public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next) {
			// リクエスト情報のログ出力
			var httpRequestData = await context.GetHttpRequestDataAsync();
			if (httpRequestData != null) {
				_logger.LogInformation("Handling request: {Method} {Url}", httpRequestData.Method, httpRequestData.Url);
			}

			// 次のミドルウェアまたは関数の実行
			await next(context);

			// レスポンス情報のログ出力
			var httpResponseData = context.GetHttpResponseData();
			if (httpResponseData != null) {
				_logger.LogInformation("Response status: {StatusCode}", httpResponseData.StatusCode);
			}
		}
	}
}

