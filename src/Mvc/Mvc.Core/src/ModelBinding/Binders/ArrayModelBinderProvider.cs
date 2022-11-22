// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

/// <summary>
/// An <see cref="IModelBinderProvider"/> for arrays.
/// </summary>
public class ArrayModelBinderProvider : IModelBinderProvider
{
    /// <inheritdoc />
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (context.Metadata.ModelType.IsArray)
        {
            if (context.Metadata is ITypedModelMetadata typedModelMetadata &&
                typedModelMetadata.CreateModelBinder(context) is IModelBinder typedModelBinder)
            {
                return typedModelBinder;
            }

            var elementType = context.Metadata.ElementMetadata!.ModelType;
            var binderType = typeof(ArrayModelBinder<>).MakeGenericType(elementType);
            var elementBinder = context.CreateBinder(context.Metadata.ElementMetadata);

            var loggerFactory = context.Services.GetRequiredService<ILoggerFactory>();
            var mvcOptions = context.Services.GetRequiredService<IOptions<MvcOptions>>().Value;
            return (IModelBinder)Activator.CreateInstance(
                binderType,
                elementBinder,
                loggerFactory,
                true /* allowValidatingTopLevelNodes */,
                mvcOptions)!;
        }

        return null;
    }
}
