// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Http.Endpoints.Results;

public abstract partial class ObjectResult : IResult
{
    /// <summary>
    /// The object result.
    /// </summary>
    public object? Value { get; protected set; }

    /// <summary>
    /// Gets the HTTP status code.
    /// </summary>
    public int? StatusCode { get; set; }

    /// <summary>
    /// Gets the value for the <c>Content-Type</c> header.
    /// </summary>
    public string? ContentType { get; set; }

    public virtual Task ExecuteAsync(HttpContext httpContext)
    {
        var logger = GetLogger(httpContext);
        Log.ObjectResultExecuting(logger, Value, StatusCode);

        if (StatusCode is { } statusCode)
        {
            httpContext.Response.StatusCode = statusCode;
        }

        ConfigureResponseHeaders(httpContext);

        if (Value is null)
        {
            return Task.CompletedTask;
        }

        OnFormatting(httpContext);
        return WriteResult(httpContext);
    }

    protected virtual void OnFormatting(HttpContext httpContext)
    {
    }

    protected virtual void ConfigureResponseHeaders(HttpContext httpContext)
    {
    }

    protected abstract ILogger GetLogger(HttpContext httpContext);

    /// <summary>
    /// Writes the response body content.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> for the response.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous write operation.</returns>
    protected abstract Task WriteResult(HttpContext httpContext);

    private static partial class Log
    {
        public static void ObjectResultExecuting(ILogger logger, object? value, int? statusCode)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                if (value is null)
                {
                    ObjectResultExecutingWithoutValue(logger, statusCode ?? StatusCodes.Status200OK);
                }
                else
                {
                    var valueType = value.GetType().FullName!;
                    ObjectResultExecuting(logger, valueType, statusCode ?? StatusCodes.Status200OK);
                }
            }
        }

        [LoggerMessage(1, LogLevel.Information, "Writing value of type '{Type}' with status code '{StatusCode}'.", EventName = "ObjectResultExecuting", SkipEnabledCheck = true)]
        private static partial void ObjectResultExecuting(ILogger logger, string type, int statusCode);

        [LoggerMessage(2, LogLevel.Information, "Executing result with status code '{StatusCode}'.", EventName = "ObjectResultExecutingWithoutValue", SkipEnabledCheck = true)]
        private static partial void ObjectResultExecutingWithoutValue(ILogger logger, int statusCode);
    }
}
