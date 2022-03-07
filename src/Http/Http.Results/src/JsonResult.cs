// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Http.Endpoints.Results;

/// <summary>
/// An action result which formats the given object as JSON.
/// </summary>
public class JsonResult : ObjectResult
{
    /// <summary>
    /// Creates a new <see cref="ObjectResult"/> instance with the provided <paramref name="value"/>.
    /// </summary>
    public JsonResult(object? value)
        : base(value)
    {
    }

    /// <summary>
    /// Creates a new <see cref="ObjectResult"/> instance with the provided <paramref name="value"/>.
    /// </summary>
    public JsonResult(object? value, int? statusCode)
        : base(value, statusCode)
    {
    }

    /// <summary>
    /// Gets or sets the serializer settings.
    /// <para>
    /// When using <c>System.Text.Json</c>, this should be an instance of <see cref="JsonSerializerOptions" />
    /// </para>
    /// <para>
    /// When using <c>Newtonsoft.Json</c>, this should be an instance of <c>JsonSerializerSettings</c>.
    /// </para>
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; init; }

    protected override ILogger GetLogger(HttpContext httpContext)
        => httpContext.RequestServices.GetRequiredService<ILogger<JsonResult>>();

    protected override Task WriteResponse(HttpContext httpContext)
        => httpContext.Response.WriteAsJsonAsync(Value, Value!.GetType(), JsonSerializerOptions, ContentType);
}
