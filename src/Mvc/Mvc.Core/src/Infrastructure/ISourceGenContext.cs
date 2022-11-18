// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Microsoft.AspNetCore.Mvc.Infrastructure;

/// <summary>
/// 
/// </summary>
public interface ISourceGenContext
{
    /// <summary>
    /// 
    /// </summary>
    IEnumerable<TypeInfo> ControllerTypes { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="controllerType"></param>
    /// <param name="controllerInfo"></param>
    /// <returns></returns>
    bool TryGetControllerInfo(
        Type controllerType,
        [NotNullWhen(true)] out ControllerInfo? controllerInfo);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="provider"></param>
    /// <param name="detailsProvider"></param>
    /// <param name="modelBindingMessageProvider"></param>
    /// <param name="modelMetadata"></param>
    /// <returns></returns>
    bool TryCreateModelMetadata(
        DefaultMetadataDetails entry,
        IModelMetadataProvider provider,
        ICompositeMetadataDetailsProvider detailsProvider,
        DefaultModelBindingMessageProvider modelBindingMessageProvider,
        [NotNullWhen(true)] out ModelMetadata? modelMetadata);
}
