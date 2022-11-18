// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

namespace Microsoft.AspNetCore.Mvc.Abstractions;

/// <summary>
/// 
/// </summary>
public abstract class ControllerInfo
{
    /// <summary>
    /// 
    /// </summary>
    public abstract ControllerActionInfo[] Actions { get; }

    /// <summary>
    /// 
    /// </summary>
    public abstract PropertyInfo[] Properties { get; }
}

/// <summary>
/// 
/// </summary>
public struct ControllerActionInfo
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="method"></param>
    /// <param name="methodInvoker"></param>
    public ControllerActionInfo(MethodInfo method, Func<object, object?[]?, object> methodInvoker)
    {
        Method = method;
        MethodInvoker = methodInvoker;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="method"></param>
    /// <param name="methodInvoker"></param>
    /// <param name="methodAwaitableInfo"></param>
    public ControllerActionInfo(MethodInfo method, Func<object, object?[]?, object> methodInvoker, ControllerActionAwaitableInfo? methodAwaitableInfo)
        : this(method, methodInvoker)
    {
        MethodAwaitableInfo = methodAwaitableInfo;
    }

    /// <summary>
    /// 
    /// </summary>
    public MethodInfo Method { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public Func<object, object?[]?, object> MethodInvoker { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public ControllerActionAwaitableInfo? MethodAwaitableInfo { get; set; }
}
