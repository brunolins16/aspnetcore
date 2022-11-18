// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Microsoft.AspNetCore.Mvc.Infrastructure;
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
    private readonly ISourceGenContext? _sourceGenContext;

    public DefaultApplicationModelProvider(
        IOptions<MvcOptions> mvcOptionsAccessor,
        IModelMetadataProvider modelMetadataProvider,
        ISourceGenContext? sourceGenContext = null)
    {
        _mvcOptions = mvcOptionsAccessor.Value;
        _modelMetadataProvider = modelMetadataProvider;
        _sourceGenContext = sourceGenContext;
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
            var controllerModelBuilder = new ControllerModelBuilder(_modelMetadataProvider, _mvcOptions.SuppressAsyncSuffixInActionNames)
                .WithControllerType(controllerType)
                .WithApplication(context.Result);

            if (_sourceGenContext?.TryGetControllerInfo(controllerType, out var controllerInfo) == true)
            {
                controllerModelBuilder = controllerModelBuilder.WithProperties(controllerInfo.Properties);
                for (var i = 0; i < controllerInfo.Actions.Length; i++)
                {
                    controllerModelBuilder = controllerModelBuilder.WithAction(
                        controllerInfo.Actions[i].Method,
                        controllerInfo.Actions[i].MethodInvoker,
                        controllerInfo.Actions[i].MethodAwaitableInfo);
                }
            }
            else
            {
                var properties = PropertyHelper.GetProperties(controllerType.AsType());

                // Coying only property info
                var propertiesInfo = new PropertyInfo[properties.Length];
                for (var i = 0; i < properties.Length; i++)
                {
                    propertiesInfo[i] = properties[i].Property;
                }

                controllerModelBuilder = controllerModelBuilder
                    .WithActions(controllerType.AsType().GetMethods())
                    .WithProperties(propertiesInfo);
            }

            context.Result.Controllers.Add(controllerModelBuilder.Build());
        }
    }

    /// <inheritdoc />
    public void OnProvidersExecuted(ApplicationModelProviderContext context)
    {
        // Intentionally empty.
    }
}
