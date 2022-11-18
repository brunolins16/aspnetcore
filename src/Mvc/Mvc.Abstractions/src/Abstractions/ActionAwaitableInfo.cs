// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.Mvc.Abstractions;

/// <summary>
/// 
/// </summary>
public readonly struct ControllerActionAwaitableInfo
{
    public Func<object, bool> AwaiterIsCompletedProperty { get; }
    public Func<object, object> AwaiterGetResultMethod { get; }
    public Action<object, Action> AwaiterOnCompletedMethod { get; }
    public Action<object, Action>? AwaiterUnsafeOnCompletedMethod { get; }
    public Func<object, object> GetAwaiterMethod { get; }
    public Type ResultType { get; }

    public ControllerActionAwaitableInfo(
        Type resultType,
        Func<object, object> getAwaiterMethod,
        Func<object, bool> isCompletedMethod,
        Func<object, object> getResultMethod,
        Action<object, Action> onCompletedMethod,
        Action<object, Action>? unsafeOnCompletedMethod)
    {
        AwaiterIsCompletedProperty = isCompletedMethod;
        AwaiterGetResultMethod = getResultMethod;
        AwaiterOnCompletedMethod = onCompletedMethod;
        AwaiterUnsafeOnCompletedMethod = unsafeOnCompletedMethod;
        ResultType = resultType;
        GetAwaiterMethod = getAwaiterMethod;
    }
}
