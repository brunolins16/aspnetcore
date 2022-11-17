// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Extensions.Internal;

/// <summary>
/// 
/// </summary>
/// <param name="target"></param>
/// <param name="parameters"></param>
/// <returns></returns>
public delegate object? MethodExecutor(object target, object?[]? parameters);
