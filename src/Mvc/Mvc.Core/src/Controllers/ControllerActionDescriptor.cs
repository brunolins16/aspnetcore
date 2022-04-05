// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Mvc.Controllers;

/// <summary>
/// A descriptor for an action of a controller.
/// </summary>
[DebuggerDisplay("{DisplayName}")]
public class ControllerActionDescriptor : ActionDescriptor
{
    /// <summary>
    /// The name of the controller.
    /// </summary>
    public string ControllerName { get; set; } = default!;

    /// <summary>
    /// The name of the action.
    /// </summary>
    public virtual string ActionName { get; set; } = default!;

    /// <summary>
    /// The <see cref="MethodInfo"/>.
    /// </summary>
    public MethodInfo MethodInfo { get; set; } = default!;

    /// <summary>
    /// The <see cref="TypeInfo"/> of the controller..
    /// </summary>
    public TypeInfo ControllerTypeInfo { get; set; } = default!;

    // Cache entry so we can avoid an external cache
    internal ControllerActionInvokerCacheEntry? CacheEntry { get; set; }

    /// <summary>
    /// Gets a flag that Indicates if the controller is configured to
    /// have an HTTP API behavior (<see cref="IApiBehaviorMetadata"/>).
    /// </summary>
    internal bool HasApiBehavior { get; set; }

    /// <inheritdoc />
    public override string? DisplayName
    {
        get
        {
            if (base.DisplayName == null && ControllerTypeInfo != null && MethodInfo != null)
            {
                base.DisplayName = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}.{1} ({2})",
                    TypeNameHelper.GetTypeDisplayName(ControllerTypeInfo),
                    MethodInfo.Name,
                    ControllerTypeInfo.Assembly.GetName().Name);
            }

            return base.DisplayName!;
        }

        set
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            base.DisplayName = value;
        }
    }

}
