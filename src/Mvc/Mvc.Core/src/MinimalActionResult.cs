// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Mvc;

internal class MinimalActionResult : ActionResult
{
    public IResult Result { get; private set;}

    public MinimalActionResult(IResult result)
    {
        Result = result;
    }


    /// <inheritdoc/>
    public override Task ExecuteResultAsync(ActionContext context)
    {
        return Result.ExecuteAsync(context.HttpContext);
    }
}
