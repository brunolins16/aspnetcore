// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// A non-buildable <see cref="IWebHostBuilder"/> for <see cref="WebApplicationBuilder"/>.
    /// Use <see cref="WebApplicationBuilder.Build"/> to build the <see cref="WebApplicationBuilder"/>.
    /// </summary>
    public sealed class ConfigureWebHostBuilder : IWebHostBuilder
    {
        private readonly WebHostEnvironment _environment;
        private readonly ConfigurationManager _configuration;
        private readonly Dictionary<string, string?> _settings = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConfigureHostBuilder _hostBuilder;

        private readonly WebHostBuilderContext _context;

        internal ConfigureWebHostBuilder(ConfigurationManager configuration, WebHostEnvironment environment, ConfigureHostBuilder hostBuilder)
        {
            _configuration = configuration;
            _environment = environment;
            _hostBuilder = hostBuilder;

            _context = new WebHostBuilderContext
            {
                Configuration = _configuration,
                HostingEnvironment = _environment
            };
        }

        IWebHost IWebHostBuilder.Build()
        {
            throw new NotSupportedException($"Call {nameof(WebApplicationBuilder)}.{nameof(WebApplicationBuilder.Build)}() instead.");
        }

        /// <inheritdoc />
        public IWebHostBuilder ConfigureAppConfiguration(Action<WebHostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            // Run these immediately so that they are observable by the imperative code
            configureDelegate(_context, _configuration);
            _environment.ApplyConfigurationSettings(_configuration);
            return this;
        }

        /// <inheritdoc />
        public IWebHostBuilder ConfigureServices(Action<WebHostBuilderContext, IServiceCollection> configureServices)
        {
            _hostBuilder.ConfigureServices((context, services) =>
            {
                configureServices(_context, services);
            });
            //// Run these immediately so that they are observable by the imperative code
            //configureServices(_context, _services);
            return this;
        }

        /// <inheritdoc />
        public IWebHostBuilder ConfigureServices(Action<IServiceCollection> configureServices)
        {
            return ConfigureServices((WebHostBuilderContext context, IServiceCollection services) => configureServices(services));
        }

        /// <inheritdoc />
        public string? GetSetting(string key)
        {
            _settings.TryGetValue(key, out var value);
            return value;
        }

        /// <inheritdoc />
        public IWebHostBuilder UseSetting(string key, string? value)
        {
            _settings[key] = value;

            // All properties on IWebHostEnvironment are non-nullable.
            if (value is null)
            {
                return this;
            }

            if (string.Equals(key, WebHostDefaults.ApplicationKey, StringComparison.OrdinalIgnoreCase))
            {
                _environment.ApplicationName = value;
            }
            else if (string.Equals(key, WebHostDefaults.ContentRootKey, StringComparison.OrdinalIgnoreCase))
            {
                _environment.ContentRootPath = value;
            }
            else if (string.Equals(key, WebHostDefaults.EnvironmentKey, StringComparison.OrdinalIgnoreCase))
            {
                _environment.EnvironmentName = value;
            }
            else if (string.Equals(key, WebHostDefaults.WebRootKey, StringComparison.OrdinalIgnoreCase))
            {
                _environment.WebRootPath = value;
            }

            return this;
        }

        internal void ApplySettings(IWebHostBuilder webHostBuilder)
        {
            foreach (var (key, value) in _settings)
            {
                webHostBuilder.UseSetting(key, value);
            }
        }
    }
}