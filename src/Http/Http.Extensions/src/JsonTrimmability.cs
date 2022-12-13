// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.Http;

internal static class JsonTrimmability
{
    private const string EnsureJsonTrimmabilityKey = "Microsoft.AspNetCore.EnsureJsonTrimmability";

    internal static bool IsTrimmable { get; } = AppContext.TryGetSwitch(EnsureJsonTrimmabilityKey, out var enabled) && enabled;
}
