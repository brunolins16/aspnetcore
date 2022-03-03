// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.Results;

using Microsoft.AspNetCore.Http;

public sealed class BadRequestResult : JsonResult
{
    public BadRequestResult(object? error)
        : base(error, StatusCodes.Status400BadRequest)
    {
    }
}
