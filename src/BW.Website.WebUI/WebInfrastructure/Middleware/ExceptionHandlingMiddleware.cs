using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Primitives;
using System.Net;
using System.Text.Json;

namespace BW.Website.WebUI.WebInfrastructure.Middleware;

public sealed class ExceptionHandlingMiddleware
{
	private const string CorrelationIdHeaderName = "X-Correlation-ID";

	private readonly RequestDelegate _next;
	private readonly ILogger<ExceptionHandlingMiddleware> _logger;
	private readonly ICompositeViewEngine _viewEngine;
	private readonly ITempDataDictionaryFactory _tempDataFactory;

	public ExceptionHandlingMiddleware(
		RequestDelegate next,
		ILogger<ExceptionHandlingMiddleware> logger,
		ICompositeViewEngine viewEngine,
		ITempDataDictionaryFactory tempDataFactory)
	{
		_next = next;
		_logger = logger;
		_viewEngine = viewEngine;
		_tempDataFactory = tempDataFactory;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		var correlationId = EnsureCorrelationId(context);
		var requestId = context.TraceIdentifier;

		using (_logger.BeginScope(new Dictionary<string, object?>
		{
			["CorrelationId"] = correlationId,
			["RequestId"] = requestId,
			["RequestPath"] = context.Request.Path.Value
		}))
		{
			try
			{
				await _next(context);
			}
			catch (ValidationException ex)
			{
				_logger.LogWarning(ex,
					"Validation error occurred. CorrelationId: {CorrelationId}, RequestId: {RequestId}",
					correlationId, requestId);

				if (ResponseAlreadyStarted(context))
				{
					throw;
				}

				if (IsHtmlRequest(context))
				{
					await RenderErrorViewAsync(
						context,
						viewName: "ValidationError",
						model: ex.Errors,
						correlationId: correlationId);
				}
				else
				{
					await WriteJsonValidationErrorAsync(context, ex, correlationId);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex,
					"Unhandled exception occurred. CorrelationId: {CorrelationId}, RequestId: {RequestId}",
					correlationId, requestId);

				if (ResponseAlreadyStarted(context))
				{
					throw;
				}

				if (IsHtmlRequest(context))
				{
					await RenderErrorViewAsync(
						context,
						viewName: "Error",
						model: ex,
						correlationId: correlationId);
				}
				else
				{
					await WriteJsonInternalErrorAsync(context, correlationId);
				}
			}
		}
	}

	private static string EnsureCorrelationId(HttpContext context)
	{
		string correlationId;

		if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out StringValues values) &&
			!StringValues.IsNullOrEmpty(values))
		{
			correlationId = values.First()!;
		}
		else
		{
			// Fall back to ASP.NET Core's TraceIdentifier if no header is supplied
			correlationId = context.TraceIdentifier;
		}

		// Always echo correlation ID back to the client
		context.Response.Headers[CorrelationIdHeaderName] = correlationId;

		return correlationId;
	}

	private static bool IsHtmlRequest(HttpContext context)
	{
		var accept = context.Request.Headers["Accept"].ToString();

		// Heuristic: if the client explicitly accepts HTML, treat as MVC/browser request
		return accept.Contains("text/html", StringComparison.OrdinalIgnoreCase);
	}

	private static bool ResponseAlreadyStarted(HttpContext context)
	{
		return context.Response.HasStarted;
	}

	private async Task RenderErrorViewAsync(
		HttpContext context,
		string viewName,
		object model,
		string correlationId)
	{
		context.Response.Clear();
		context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
		context.Response.ContentType = "text/html; charset=utf-8";

		var actionContext = new ActionContext(
			context,
			context.GetRouteData() ?? new RouteData(),
			new ActionDescriptor());

		var viewResult = _viewEngine.FindView(actionContext, viewName, isMainPage: true);

		if (!viewResult.Success || viewResult.View is null)
		{
			// Fallback simple HTML if view is missing
			await context.Response.WriteAsync($"""
                <html>
                    <head><title>Error</title></head>
                    <body>
                        <h2>There was a problem rendering the error view.</h2>
                        <p>Expected view: {viewName}</p>
                        <p>CorrelationId: {correlationId}</p>
                    </body>
                </html>
                """);
			return;
		}

		var viewData = new ViewDataDictionary<object>(
			metadataProvider: new EmptyModelMetadataProvider(),
			modelState: new ModelStateDictionary())
		{
			Model = model
		};

		// Expose correlation ID to the view
		viewData["CorrelationId"] = correlationId;

		var tempData = _tempDataFactory.GetTempData(context);

		await using var writer = new StringWriter();

		var viewContext = new ViewContext(
			actionContext,
			viewResult.View,
			viewData,
			tempData,
			writer,
			new HtmlHelperOptions());

		await viewResult.View.RenderAsync(viewContext);

		await context.Response.WriteAsync(writer.ToString());
	}

	private async Task WriteJsonValidationErrorAsync(
		HttpContext context,
		ValidationException ex,
		string correlationId)
	{
		context.Response.Clear();
		context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
		context.Response.ContentType = "application/json; charset=utf-8";

		var payload = new
		{
			title = "Validation failed.",
			status = (int)HttpStatusCode.BadRequest,
			correlationId,
			errors = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
		};

		await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
	}

	private async Task WriteJsonInternalErrorAsync(
		HttpContext context,
		string correlationId)
	{
		context.Response.Clear();
		context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
		context.Response.ContentType = "application/json; charset=utf-8";

		var payload = new
		{
			title = "An unexpected error occurred.",
			status = (int)HttpStatusCode.InternalServerError,
			correlationId
		};

		await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
	}
}
