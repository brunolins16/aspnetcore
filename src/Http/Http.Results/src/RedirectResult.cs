// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Internal;

namespace Microsoft.AspNetCore.Http.Endpoints.Results;

public sealed class RedirectResult : RedirectResultBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectResult"/> class with the values
    /// provided.
    /// </summary>
    /// <param name="url">The URL to redirect to.</param>
    /// <param name="permanent">Specifies whether the redirect should be permanent (301) or temporary (302).</param>
    /// <param name="preserveMethod">If set to true, make the temporary redirect (307) or permanent redirect (308) preserve the initial request method.</param>
    public RedirectResult(string url, bool permanent, bool preserveMethod)
        : base(permanent, preserveMethod)
    {
        if (url == null)
        {
            throw new ArgumentNullException(nameof(url));
        }

        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentException("Argument cannot be null or empty", nameof(url));
        }

        Url = url;
    }

    /// <summary>
    /// Gets or sets the URL to redirect to.
    /// </summary>
    public string Url { get; }

    public override string GetLocation(HttpContext httpContext)
        => SharedUrlHelper.IsLocalUrl(Url) ? SharedUrlHelper.Content(httpContext, Url) : Url;
}
