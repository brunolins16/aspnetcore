// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Http.Endpoints.Results;

public abstract partial class RedirectResultBase : StatusCodeResult
{
    protected RedirectResultBase(bool permanent, bool preserveMethod)
    {
        Permanent = permanent;
        PreserveMethod = preserveMethod;
        StatusCode = Permanent
            ? StatusCodes.Status308PermanentRedirect
            : StatusCodes.Status307TemporaryRedirect;
    }

    /// <summary>
    /// Gets or sets the value that specifies that the redirect should be permanent if true or temporary if false.
    /// </summary>
    public bool Permanent { get; }

    /// <summary>
    /// Gets or sets an indication that the redirect preserves the initial request method.
    /// </summary>
    public bool PreserveMethod { get; }

    /// <inheritdoc />
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        var logger = httpContext.RequestServices.GetRequiredService<ILogger<RedirectResultBase>>();

        // IsLocalUrl is called to handle URLs starting with '~/'.
        var destinationUrl = GetLocation(httpContext);
        Log.RedirectResultExecuting(logger, destinationUrl);

        if (PreserveMethod)
        {
            await base.ExecuteAsync(httpContext);
            httpContext.Response.Headers.Location = destinationUrl;
        }
        else
        {
            httpContext.Response.Redirect(destinationUrl, Permanent);
        }
    }

    public abstract string GetLocation(HttpContext httpContext);

    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Information,
            "Executing RedirectResult, redirecting to {Destination}.",
            EventName = "RedirectResultExecuting")]
        public static partial void RedirectResultExecuting(ILogger logger, string destination);
    }
}
