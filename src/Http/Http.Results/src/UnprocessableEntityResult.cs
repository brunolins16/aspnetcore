// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.Http.Endpoints.Results;

public sealed class UnprocessableEntityResult : ObjectResult
{
    public UnprocessableEntityResult(object? error)
        : base(error, StatusCodes.Status422UnprocessableEntity)
    {
    }
}
