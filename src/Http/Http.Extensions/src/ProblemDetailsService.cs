// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;

namespace Microsoft.AspNetCore.Http;

internal sealed class ProblemDetailsService : IProblemDetailsService
{
    private readonly IProblemDetailsWriter[] _writers;
    private readonly DefaultProblemDetailsWriter _defaultWriter;

    public ProblemDetailsService(
        IEnumerable<IProblemDetailsWriter> writers,
        DefaultProblemDetailsWriter defaultProblemDetailsWriter)
    {
        _writers = writers.ToArray();
        _defaultWriter = defaultProblemDetailsWriter;
    }

    public async ValueTask WriteAsync(ProblemDetailsContext context)
    {
        if (!await TryWriteAsync(context))
        {
            throw new InvalidOperationException("Unable to find a registered `IProblemDetailsWriter` that can write to the given context.");
        }
    }

    public async ValueTask<bool> TryWriteAsync(ProblemDetailsContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.ProblemDetails);
        ArgumentNullException.ThrowIfNull(context.HttpContext);

        // Try to write using all registered writers
        // sequentially and stop at the first one that
        // `canWrite`.
        for (var i = 0; i < _writers.Length; i++)
        {
            var selectedWriter = _writers[i];
            if (selectedWriter.CanWrite(context))
            {
                await selectedWriter.WriteAsync(context);
                return true;
            }
        }

        if (_defaultWriter.CanWrite(context))
        {
            await _defaultWriter.WriteAsync(context);
            return true;
        }

        return false;
    }
}
