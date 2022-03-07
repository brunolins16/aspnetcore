// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.Http.Endpoints.Results;

using Microsoft.AspNetCore.Mvc;

public sealed class ProblemResult : ObjectResult
{
    public ProblemResult(ProblemDetails problemDetails)
        : base(problemDetails)
    {
    }
}
