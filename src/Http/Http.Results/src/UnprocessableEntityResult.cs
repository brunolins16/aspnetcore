// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.Http.Endpoints.Results;

using Microsoft.AspNetCore.Http;

public sealed class UnprocessableEntityResult : JsonResult
{
    public UnprocessableEntityResult(object? error)
        : base(error, StatusCodes.Status422UnprocessableEntity)
    {
    }
}
