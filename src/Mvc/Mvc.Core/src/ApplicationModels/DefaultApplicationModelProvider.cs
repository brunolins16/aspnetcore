// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels;

#pragma warning disable CA1852 // Seal internal types
internal class DefaultApplicationModelProvider : IApplicationModelProvider
#pragma warning restore CA1852 // Seal internal types
{
    private readonly MvcOptions _mvcOptions;
    private readonly IModelMetadataProvider _modelMetadataProvider;

    public DefaultApplicationModelProvider(
        IOptions<MvcOptions> mvcOptionsAccessor,
        IModelMetadataProvider modelMetadataProvider)
    {
        _mvcOptions = mvcOptionsAccessor.Value;
        _modelMetadataProvider = modelMetadataProvider;
    }

    /// <inheritdoc />
    public int Order => -1000;

    /// <inheritdoc />
    public void OnProvidersExecuting(ApplicationModelProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        foreach (var filter in _mvcOptions.Filters)
        {
            context.Result.Filters.Add(filter);
        }

        foreach (var controllerType in context.ControllerTypes)
        {
            if (context.Result.Controllers.Any(c => c.ControllerType.IsAssignableFrom(controllerType)))
            {
                continue;
            }

            var properties = PropertyHelper.GetProperties(controllerType.AsType());

            // Coying only property info
            var propertiesInfo = new PropertyInfo[properties.Length];
            for (var i = 0; i < properties.Length; i++)
            {
                propertiesInfo[i] = properties[i].Property;
            }

            var controllerModelBuilder = new ControllerModelBuilder(_modelMetadataProvider, _mvcOptions.SuppressAsyncSuffixInActionNames)
                .WithControllerType(controllerType)
                .WithApplication(context.Result)
                .WithActions(controllerType.AsType().GetMethods())
                .WithProperties(propertiesInfo);

            context.Result.Controllers.Add(controllerModelBuilder.Build());
        }
    }

    /// <inheritdoc />
    public void OnProvidersExecuted(ApplicationModelProviderContext context)
    {
        // Intentionally empty.
    }
}
