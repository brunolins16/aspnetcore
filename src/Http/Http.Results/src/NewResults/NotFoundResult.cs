// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.Results;

using Microsoft.AspNetCore.Http;

public sealed class NotFoundResult : JsonResult
{
    public NotFoundResult(object? value)
        : base(value, StatusCodes.Status404NotFound)
    {
    }
}
