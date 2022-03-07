// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.Http.Endpoints.Results;

public sealed class NotFoundResult : ObjectResult
{
    public NotFoundResult(object? value)
        : base(value, StatusCodes.Status404NotFound)
    {
    }
}
