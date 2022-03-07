// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.Http.Endpoints.Results;

using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

public sealed class ProblemResult : ObjectResult
{
    public ProblemResult(ProblemDetails problemDetails)
        : base(problemDetails)
    {
        ApplyProblemDetailsDefaults(problemDetails);
    }

    private void ApplyProblemDetailsDefaults(ProblemDetails problemDetails)
    {
        ContentType = "application/problem+json";

        // We allow StatusCode to be specified either on ProblemDetails or on the ObjectResult and use it to configure the other.
        // This lets users write <c>return Conflict(new Problem("some description"))</c>
        // or <c>return Problem("some-problem", 422)</c> and have the response have consistent fields.
        if (problemDetails.Status is null)
        {
            if (StatusCode is not null)
            {
                problemDetails.Status = StatusCode;
            }
            else
            {
                problemDetails.Status = problemDetails is HttpValidationProblemDetails ?
                    StatusCodes.Status400BadRequest :
                    StatusCodes.Status500InternalServerError;
            }
        }

        if (StatusCode is null)
        {
            StatusCode = problemDetails.Status;
        }

        if (ProblemDetailsDefaults.Defaults.TryGetValue(problemDetails.Status.Value, out var defaults))
        {
            problemDetails.Title ??= defaults.Title;
            problemDetails.Type ??= defaults.Type;
        }
    }
}
