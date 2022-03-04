// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.Http.Endpoints.Results;

using Microsoft.AspNetCore.Http;

public sealed class OkResult : JsonResult
{
    public OkResult(object? value)
        : base(value, StatusCodes.Status200OK)
    {
    }
}
