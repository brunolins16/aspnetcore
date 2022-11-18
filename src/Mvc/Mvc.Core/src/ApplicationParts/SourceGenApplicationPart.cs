// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Microsoft.AspNetCore.Mvc.ApplicationParts;

internal sealed class SourceGenApplicationPart : ApplicationPart, IApplicationPartTypeProvider
{
    private readonly ISourceGenContext? _sourceGenContext;

    public SourceGenApplicationPart(ISourceGenContext? sourceGenContext = null)
    {
        _sourceGenContext = sourceGenContext;
    }

    public IEnumerable<TypeInfo> Types => _sourceGenContext?.ControllerTypes ?? Array.Empty<TypeInfo>();

    public override string Name => nameof(SourceGenApplicationPart);
}
