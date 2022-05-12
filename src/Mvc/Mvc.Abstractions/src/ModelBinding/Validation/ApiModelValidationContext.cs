// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

/// <summary>
/// A context object for <see cref="IModelValidator"/>.
/// </summary>
internal class ApiModelValidationContext : ModelValidationContext
{
    /// <summary>
    /// Create a new instance of <see cref="ModelValidationContext"/>.
    /// </summary>
    /// <param name="actionContext">The <see cref="ActionContext"/> for validation.</param>
    /// <param name="modelMetadata">The <see cref="ModelMetadata"/> for validation.</param>
    /// <param name="metadataProvider">The <see cref="IModelMetadataProvider"/> to be used in validation.</param>
    /// <param name="container">The model container.</param>
    /// <param name="model">The model to be validated.</param>
    public ApiModelValidationContext(
        ActionContext actionContext,
        ModelMetadata modelMetadata,
        IModelMetadataProvider metadataProvider,
        object? container,
        object? model)
        : base(actionContext, modelMetadata, metadataProvider, container, model)
    {
    }

    internal override string ModelDisplayName
        => ModelMetadata.ValidationModelName ?? ModelMetadata.GetDisplayName();
}
