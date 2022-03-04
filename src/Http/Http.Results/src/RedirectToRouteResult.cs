// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Http.Endpoints.Results;

/// <summary>
/// An <see cref="IResult"/> that returns a Found (302), Moved Permanently (301), Temporary Redirect (307),
/// or Permanent Redirect (308) response with a Location header.
/// Targets a registered route.
/// </summary>
public sealed class RedirectToRouteResult : RedirectResultBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectToRouteResult"/> with the values
    /// provided.
    /// </summary>
    /// <param name="routeValues">The parameters for the route.</param>
    public RedirectToRouteResult(object? routeValues)
        : this(routeName: null, routeValues: routeValues)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectToRouteResult"/> with the values
    /// provided.
    /// </summary>
    /// <param name="routeName">The name of the route.</param>
    /// <param name="routeValues">The parameters for the route.</param>
    public RedirectToRouteResult(
        string? routeName,
        object? routeValues)
        : this(routeName, routeValues, permanent: false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectToRouteResult"/> with the values
    /// provided.
    /// </summary>
    /// <param name="routeName">The name of the route.</param>
    /// <param name="routeValues">The parameters for the route.</param>
    /// <param name="permanent">If set to true, makes the redirect permanent (301). Otherwise a temporary redirect is used (302).</param>
    public RedirectToRouteResult(
        string? routeName,
        object? routeValues,
        bool permanent)
        : this(routeName, routeValues, permanent, fragment: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectToRouteResult"/> with the values
    /// provided.
    /// </summary>
    /// <param name="routeName">The name of the route.</param>
    /// <param name="routeValues">The parameters for the route.</param>
    /// <param name="permanent">If set to true, makes the redirect permanent (301). Otherwise a temporary redirect is used (302).</param>
    /// <param name="preserveMethod">If set to true, make the temporary redirect (307) or permanent redirect (308) preserve the initial request method.</param>
    public RedirectToRouteResult(
        string? routeName,
        object? routeValues,
        bool permanent,
        bool preserveMethod)
        : this(routeName, routeValues, permanent, preserveMethod, fragment: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectToRouteResult"/> with the values
    /// provided.
    /// </summary>
    /// <param name="routeName">The name of the route.</param>
    /// <param name="routeValues">The parameters for the route.</param>
    /// <param name="fragment">The fragment to add to the URL.</param>
    public RedirectToRouteResult(
        string? routeName,
        object? routeValues,
        string? fragment)
        : this(routeName, routeValues, permanent: false, fragment: fragment)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectToRouteResult"/> with the values
    /// provided.
    /// </summary>
    /// <param name="routeName">The name of the route.</param>
    /// <param name="routeValues">The parameters for the route.</param>
    /// <param name="permanent">If set to true, makes the redirect permanent (301). Otherwise a temporary redirect is used (302).</param>
    /// <param name="fragment">The fragment to add to the URL.</param>
    public RedirectToRouteResult(
        string? routeName,
        object? routeValues,
        bool permanent,
        string? fragment)
        : this(routeName, routeValues, permanent, preserveMethod: false, fragment: fragment)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectToRouteResult"/> with the values
    /// provided.
    /// </summary>
    /// <param name="routeName">The name of the route.</param>
    /// <param name="routeValues">The parameters for the route.</param>
    /// <param name="permanent">If set to true, makes the redirect permanent (301). Otherwise a temporary redirect is used (302).</param>
    /// <param name="preserveMethod">If set to true, make the temporary redirect (307) or permanent redirect (308) preserve the initial request method.</param>
    /// <param name="fragment">The fragment to add to the URL.</param>
    public RedirectToRouteResult(
        string? routeName,
        object? routeValues,
        bool permanent,
        bool preserveMethod,
        string? fragment)
        : base(permanent, preserveMethod)
    {
        RouteName = routeName;
        RouteValues = routeValues == null ? null : new RouteValueDictionary(routeValues);
        Fragment = fragment;
    }

    /// <summary>
    /// Gets or sets the name of the route to use for generating the URL.
    /// </summary>
    public string? RouteName { get; }

    /// <summary>
    /// Gets or sets the route data to use for generating the URL.
    /// </summary>
    public RouteValueDictionary? RouteValues { get; }

    /// <summary>
    /// Gets or sets the fragment to add to the URL.
    /// </summary>
    public string? Fragment { get; }

    public override string GetLocation(HttpContext httpContext)
    {
        var linkGenerator = httpContext.RequestServices.GetRequiredService<LinkGenerator>();

        var destinationUrl = linkGenerator.GetUriByRouteValues(
            httpContext,
            RouteName,
            RouteValues,
            fragment: Fragment == null ? FragmentString.Empty : new FragmentString("#" + Fragment));

        if (string.IsNullOrEmpty(destinationUrl))
        {
            throw new InvalidOperationException("No route matches the supplied values.");
        }

        return destinationUrl;
    }
}
