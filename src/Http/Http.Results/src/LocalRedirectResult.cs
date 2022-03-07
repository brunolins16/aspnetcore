// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Internal;

namespace Microsoft.AspNetCore.Http.Endpoints.Results;

/// <summary>
/// An <see cref="IResult"/> that returns a Found (302), Moved Permanently (301), Temporary Redirect (307),
/// or Permanent Redirect (308) response with a Location header to the supplied local URL.
/// </summary>
public sealed partial class LocalRedirectResult : RedirectResultBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalRedirectResult"/> class with the values
    /// provided.
    /// </summary>
    /// <param name="localUrl">The local URL to redirect to.</param>
    public LocalRedirectResult(string localUrl)
         : this(localUrl, permanent: false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalRedirectResult"/> class with the values
    /// provided.
    /// </summary>
    /// <param name="localUrl">The local URL to redirect to.</param>
    /// <param name="permanent">Specifies whether the redirect should be permanent (301) or temporary (302).</param>
    public LocalRedirectResult(string localUrl, bool permanent)
        : this(localUrl, permanent, preserveMethod: false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalRedirectResult"/> class with the values
    /// provided.
    /// </summary>
    /// <param name="localUrl">The local URL to redirect to.</param>
    /// <param name="permanent">Specifies whether the redirect should be permanent (301) or temporary (302).</param>
    /// <param name="preserveMethod">If set to true, make the temporary redirect (307) or permanent redirect (308) preserve the initial request's method.</param>
    public LocalRedirectResult(string localUrl, bool permanent, bool preserveMethod)
        : base(permanent, preserveMethod)
    {
        if (string.IsNullOrEmpty(localUrl))
        {
            throw new ArgumentException("Argument cannot be null or empty", nameof(localUrl));
        }

        Url = localUrl;
    }

    /// <summary>
    /// Gets or sets the local URL to redirect to.
    /// </summary>
    public string Url { get; }

    public override string GetLocation(HttpContext httpContext)
    {
        if (!SharedUrlHelper.IsLocalUrl(Url))
        {
            throw new InvalidOperationException("The supplied URL is not local. A URL with an absolute path is considered local if it does not have a host/authority part. URLs using virtual paths ('~/') are also local.");
        }

        return SharedUrlHelper.Content(httpContext, Url);
    }
}
