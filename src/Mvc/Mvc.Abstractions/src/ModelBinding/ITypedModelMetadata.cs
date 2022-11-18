// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.Mvc.ModelBinding;

/// <summary>
/// 
/// </summary>
public interface ITypedModelMetadata
{
    IModelBinder? CreateModelBinder(ModelBinderProviderContext context);
}
