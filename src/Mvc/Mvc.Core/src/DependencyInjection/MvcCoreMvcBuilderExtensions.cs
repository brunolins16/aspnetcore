// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions for configuring MVC using an <see cref="IMvcBuilder"/>.
/// </summary>
public static class MvcCoreMvcBuilderExtensions
{
    /// <summary>
    /// Registers an action to configure <see cref="MvcOptions"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
    /// <param name="setupAction">An <see cref="Action{MvcOptions}"/>.</param>
    /// <returns>The <see cref="IMvcBuilder"/>.</returns>
    public static IMvcBuilder AddMvcOptions(
        this IMvcBuilder builder,
        Action<MvcOptions> setupAction)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (setupAction == null)
        {
            throw new ArgumentNullException(nameof(setupAction));
        }

        builder.Services.Configure(setupAction);
        return builder;
    }

    /// <summary>
    /// Configures <see cref="JsonOptions"/> for the specified <paramref name="builder"/>.
    /// Uses default values from <c>JsonSerializerDefaults.Web</c>.
    /// </summary>
    /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
    /// <param name="configure">An <see cref="Action"/> to configure the <see cref="JsonOptions"/>.</param>
    /// <returns>The <see cref="IMvcBuilder"/>.</returns>
    public static IMvcBuilder AddJsonOptions(
        this IMvcBuilder builder,
        Action<JsonOptions> configure)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        builder.Services.Configure(configure);
        return builder;
    }

    /// <summary>
    /// Configures <see cref="FormatterMappings"/> for the specified <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
    /// <param name="setupAction">An <see cref="Action"/> to configure the <see cref="FormatterMappings"/>.</param>
    /// <returns>The <see cref="IMvcBuilder"/>.</returns>
    public static IMvcBuilder AddFormatterMappings(
        this IMvcBuilder builder,
        Action<FormatterMappings> setupAction)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (setupAction == null)
        {
            throw new ArgumentNullException(nameof(setupAction));
        }

        builder.Services.Configure<MvcOptions>((options) => setupAction(options.FormatterMappings));
        return builder;
    }

    /// <summary>
    /// Adds an <see cref="ApplicationPart"/> to the list of <see cref="ApplicationPartManager.ApplicationParts"/> on the
    /// <see cref="IMvcBuilder.PartManager"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
    /// <param name="assembly">The <see cref="Assembly"/> of the <see cref="ApplicationPart"/>.</param>
    /// <returns>The <see cref="IMvcBuilder"/>.</returns>
    public static IMvcBuilder AddApplicationPart(this IMvcBuilder builder, Assembly assembly)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (assembly == null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        builder.ConfigureApplicationPartManager(manager =>
        {
            var partFactory = ApplicationPartFactory.GetApplicationPartFactory(assembly);
            foreach (var applicationPart in partFactory.GetApplicationParts(assembly))
            {
                manager.ApplicationParts.Add(applicationPart);
            }
        });

        return builder;
    }

    /// <summary>
    /// Configures the <see cref="ApplicationPartManager"/> of the <see cref="IMvcBuilder.PartManager"/> using
    /// the given <see cref="Action{ApplicationPartManager}"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
    /// <param name="setupAction">The <see cref="Action{ApplicationPartManager}"/></param>
    /// <returns>The <see cref="IMvcBuilder"/>.</returns>
    public static IMvcBuilder ConfigureApplicationPartManager(
        this IMvcBuilder builder,
        Action<ApplicationPartManager> setupAction)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (setupAction == null)
        {
            throw new ArgumentNullException(nameof(setupAction));
        }

        setupAction(builder.PartManager);

        return builder;
    }

    /// <summary>
    /// Registers discovered controllers as services in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
    /// <returns>The <see cref="IMvcBuilder"/>.</returns>
    public static IMvcBuilder AddControllersAsServices(this IMvcBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        var feature = new ControllerFeature();
        builder.PartManager.PopulateFeature(feature);

        foreach (var controller in feature.Controllers.Select(c => c.AsType()))
        {
            builder.Services.TryAddTransient(controller, controller);
        }

        builder.Services.Replace(ServiceDescriptor.Transient<IControllerActivator, ServiceBasedControllerActivator>());

        return builder;
    }

    /// <summary>
    /// Sets the <see cref="CompatibilityVersion"/> for ASP.NET Core MVC for the application.
    /// </summary>
    /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
    /// <param name="version">The <see cref="CompatibilityVersion"/> value to configure.</param>
    /// <returns>The <see cref="IMvcBuilder"/>.</returns>
    [Obsolete("This API is obsolete and will be removed in a future version. Consider removing usages.",
        DiagnosticId = "ASP5001",
        UrlFormat = "https://aka.ms/aspnetcore-warnings/{0}")]
    public static IMvcBuilder SetCompatibilityVersion(this IMvcBuilder builder, CompatibilityVersion version)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Services.Configure<MvcCompatibilityOptions>(o => o.CompatibilityVersion = version);
        return builder;
    }

    /// <summary>
    /// Configures <see cref="ApiBehaviorOptions"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
    /// <param name="setupAction">The configure action.</param>
    /// <returns>The <see cref="IMvcBuilder"/>.</returns>
    public static IMvcBuilder ConfigureApiBehaviorOptions(
        this IMvcBuilder builder,
        Action<ApiBehaviorOptions> setupAction)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (setupAction == null)
        {
            throw new ArgumentNullException(nameof(setupAction));
        }

        builder.Services.Configure(setupAction);

        return builder;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static IMvcBuilder AddMvcContext(
        this IMvcBuilder builder,
        ISourceGenContext? context = null)
    {
        if (context != null)
        {
            // TODO: Verify
            _ = builder.Services.Configure<MvcOptions>(options => options.SourceGenContext = context);

            builder = builder.ConfigureApplicationPartManager((appPartManager) =>
            {
                for (var i = appPartManager.ApplicationParts.Count - 1; i >= 0; i--)
                {
                    if (appPartManager.ApplicationParts[i] is SourceGenApplicationPart)
                    {
                        appPartManager.ApplicationParts.RemoveAt(i);
                    }
                }

                appPartManager.ApplicationParts.Add(new SourceGenApplicationPart(context));
            });

            _ = builder.Services.AddSingleton(context);
        }

        return builder;
    }
}
