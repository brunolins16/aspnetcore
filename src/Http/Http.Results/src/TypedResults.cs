// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO.Pipelines;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Http;

/// <summary>
/// A factory for <see cref="IResult"/>.
/// </summary>
public static partial class Results
{
    public static class Typed
    {
        /// <summary>
        /// Creates an <see cref="IResult"/> that on execution invokes <see cref="AuthenticationHttpContextExtensions.ChallengeAsync(HttpContext, string?, AuthenticationProperties?)" />.
        /// <para>
        /// The behavior of this method depends on the <see cref="IAuthenticationService"/> in use.
        /// <see cref="StatusCodes.Status401Unauthorized"/> and <see cref="StatusCodes.Status403Forbidden"/>
        /// are among likely status results.
        /// </para>
        /// </summary>
        /// <param name="properties"><see cref="AuthenticationProperties"/> used to perform the authentication
        /// challenge.</param>
        /// <param name="authenticationSchemes">The authentication schemes to challenge.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static Challenge Challenge(
            AuthenticationProperties? properties = null,
            IList<string>? authenticationSchemes = null)
            => new(authenticationSchemes: authenticationSchemes ?? Array.Empty<string>(), properties);

        /// <summary>
        /// Creates a <see cref="IResult"/> that on execution invokes <see cref="AuthenticationHttpContextExtensions.ForbidAsync(HttpContext, string?, AuthenticationProperties?)"/>.
        /// <para>
        /// By default, executing this result returns a <see cref="StatusCodes.Status403Forbidden"/>. Some authentication schemes, such as cookies,
        /// will convert <see cref="StatusCodes.Status403Forbidden"/> to a redirect to show a login page.
        /// </para>
        /// </summary>
        /// <param name="properties"><see cref="AuthenticationProperties"/> used to perform the authentication
        /// challenge.</param>
        /// <param name="authenticationSchemes">The authentication schemes to challenge.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        /// <remarks>
        /// Some authentication schemes, such as cookies, will convert <see cref="StatusCodes.Status403Forbidden"/> to
        /// a redirect to show a login page.
        /// </remarks>
        public static Forbid Forbid(AuthenticationProperties? properties = null, IList<string>? authenticationSchemes = null)
            => new(authenticationSchemes: authenticationSchemes ?? Array.Empty<string>(), properties);

        /// <summary>
        /// Creates an <see cref="IResult"/> that on execution invokes <see cref="AuthenticationHttpContextExtensions.SignInAsync(HttpContext, string?, ClaimsPrincipal, AuthenticationProperties?)" />.
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/> containing the user claims.</param>
        /// <param name="properties"><see cref="AuthenticationProperties"/> used to perform the sign-in operation.</param>
        /// <param name="authenticationScheme">The authentication scheme to use for the sign-in operation.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static SignIn SignIn(
            ClaimsPrincipal principal,
            AuthenticationProperties? properties = null,
            string? authenticationScheme = null)
            => new(principal, authenticationScheme, properties);

        /// <summary>
        /// Creates an <see cref="IResult"/> that on execution invokes <see cref="AuthenticationHttpContextExtensions.SignOutAsync(HttpContext, string?, AuthenticationProperties?)" />.
        /// </summary>
        /// <param name="properties"><see cref="AuthenticationProperties"/> used to perform the sign-out operation.</param>
        /// <param name="authenticationSchemes">The authentication scheme to use for the sign-out operation.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static SignOut SignOut(AuthenticationProperties? properties = null, IList<string>? authenticationSchemes = null)
            => new(authenticationSchemes ?? Array.Empty<string>(), properties);

        /// <summary>
        /// Writes the <paramref name="content"/> string to the HTTP response.
        /// <para>
        /// This is an alias for <see cref="Text(string, string?, Encoding?)"/>.
        /// </para>
        /// </summary>
        /// <param name="content">The content to write to the response.</param>
        /// <param name="contentType">The content type (MIME type).</param>
        /// <param name="contentEncoding">The content encoding.</param>
        /// <returns>The created <see cref="IResult"/> object for the response.</returns>
        /// <remarks>
        /// If encoding is provided by both the 'charset' and the <paramref name="contentEncoding"/> parameters, then
        /// the <paramref name="contentEncoding"/> parameter is chosen as the final encoding.
        /// </remarks>
        public static Content Content(string content, string? contentType = null, Encoding? contentEncoding = null)
            => Text(content, contentType, contentEncoding);

        /// <summary>
        /// Writes the <paramref name="content"/> string to the HTTP response.
        /// <para>
        /// This is an alias for <see cref="Content(string, string?, Encoding?)"/>.
        /// </para>
        /// </summary>
        /// <param name="content">The content to write to the response.</param>
        /// <param name="contentType">The content type (MIME type).</param>
        /// <param name="contentEncoding">The content encoding.</param>
        /// <returns>The created <see cref="IResult"/> object for the response.</returns>
        /// <remarks>
        /// If encoding is provided by both the 'charset' and the <paramref name="contentEncoding"/> parameters, then
        /// the <paramref name="contentEncoding"/> parameter is chosen as the final encoding.
        /// </remarks>
        public static Content Text(string content, string? contentType = null, Encoding? contentEncoding = null)
        {
            MediaTypeHeaderValue? mediaTypeHeaderValue = null;
            if (contentType is not null)
            {
                mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(contentType);
                mediaTypeHeaderValue.Encoding = contentEncoding ?? mediaTypeHeaderValue.Encoding;
            }

            return new(content, mediaTypeHeaderValue?.ToString());
        }

        /// <summary>
        /// Writes the <paramref name="content"/> string to the HTTP response.
        /// </summary>
        /// <param name="content">The content to write to the response.</param>
        /// <param name="contentType">The content type (MIME type).</param>
        /// <returns>The created <see cref="IResult"/> object for the response.</returns>
        public static Content Content(string content, MediaTypeHeaderValue contentType)
            => new(content, contentType.ToString());

        /// <summary>
        /// Creates a <see cref="IResult"/> that serializes the specified <paramref name="data"/> object to JSON.
        /// </summary>
        /// <param name="data">The object to write as JSON.</param>
        /// <param name="options">The serializer options to use when serializing the value.</param>
        /// <param name="contentType">The content-type to set on the response.</param>
        /// <param name="statusCode">The status code to set on the response.</param>
        /// <returns>The created <see cref="IResult"/> that serializes the specified <paramref name="data"/>
        /// as JSON format for the response.</returns>
        /// <remarks>Callers should cache an instance of serializer settings to avoid
        /// recreating cached data with each call.</remarks>
        public static Json<TValue> Json<TValue>(TValue? data, JsonSerializerOptions? options = null, string? contentType = null, int? statusCode = null)
            => new(data, statusCode, options)
            {
                ContentType = contentType,
            };

        /// <summary>
        /// Writes the byte-array content to the response.
        /// <para>
        /// This supports range requests (<see cref="StatusCodes.Status206PartialContent"/> or
        /// <see cref="StatusCodes.Status416RangeNotSatisfiable"/> if the range is not satisfiable).
        /// </para>
        /// <para>
        /// This API is an alias for <see cref="Bytes(byte[], string, string?, bool, DateTimeOffset?, EntityTagHeaderValue?)"/>.</para>
        /// </summary>
        /// <param name="fileContents">The file contents.</param>
        /// <param name="contentType">The Content-Type of the file.</param>
        /// <param name="fileDownloadName">The suggested file name.</param>
        /// <param name="enableRangeProcessing">Set to <c>true</c> to enable range requests processing.</param>
        /// <param name="lastModified">The <see cref="DateTimeOffset"/> of when the file was last modified.</param>
        /// <param name="entityTag">The <see cref="EntityTagHeaderValue"/> associated with the file.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
        public static FileContent File(
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
                byte[] fileContents,
            string? contentType = null,
            string? fileDownloadName = null,
            bool enableRangeProcessing = false,
            DateTimeOffset? lastModified = null,
            EntityTagHeaderValue? entityTag = null)
            => new(fileContents, contentType)
            {
                FileDownloadName = fileDownloadName,
                EnableRangeProcessing = enableRangeProcessing,
                LastModified = lastModified,
                EntityTag = entityTag,
            };

        /// <summary>
        /// Writes the byte-array content to the response.
        /// <para>
        /// This supports range requests (<see cref="StatusCodes.Status206PartialContent"/> or
        /// <see cref="StatusCodes.Status416RangeNotSatisfiable"/> if the range is not satisfiable).
        /// </para>
        /// <para>
        /// This API is an alias for <see cref="File(byte[], string, string?, bool, DateTimeOffset?, EntityTagHeaderValue?)"/>.</para>
        /// </summary>
        /// <param name="contents">The file contents.</param>
        /// <param name="contentType">The Content-Type of the file.</param>
        /// <param name="fileDownloadName">The suggested file name.</param>
        /// <param name="enableRangeProcessing">Set to <c>true</c> to enable range requests processing.</param>
        /// <param name="lastModified">The <see cref="DateTimeOffset"/> of when the file was last modified.</param>
        /// <param name="entityTag">The <see cref="EntityTagHeaderValue"/> associated with the file.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static FileContent Bytes(
            byte[] contents,
            string? contentType = null,
            string? fileDownloadName = null,
            bool enableRangeProcessing = false,
            DateTimeOffset? lastModified = null,
            EntityTagHeaderValue? entityTag = null)
            => new(contents, contentType)
            {
                FileDownloadName = fileDownloadName,
                EnableRangeProcessing = enableRangeProcessing,
                LastModified = lastModified,
                EntityTag = entityTag,
            };

        /// <summary>
        /// Writes the byte-array content to the response.
        /// <para>
        /// This supports range requests (<see cref="StatusCodes.Status206PartialContent"/> or
        /// <see cref="StatusCodes.Status416RangeNotSatisfiable"/> if the range is not satisfiable).
        /// </para>
        /// </summary>
        /// <param name="contents">The file contents.</param>
        /// <param name="contentType">The Content-Type of the file.</param>
        /// <param name="fileDownloadName">The suggested file name.</param>
        /// <param name="enableRangeProcessing">Set to <c>true</c> to enable range requests processing.</param>
        /// <param name="lastModified">The <see cref="DateTimeOffset"/> of when the file was last modified.</param>
        /// <param name="entityTag">The <see cref="EntityTagHeaderValue"/> associated with the file.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
        public static FileContent Bytes(
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
            ReadOnlyMemory<byte> contents,
            string? contentType = null,
            string? fileDownloadName = null,
            bool enableRangeProcessing = false,
            DateTimeOffset? lastModified = null,
            EntityTagHeaderValue? entityTag = null)
            => new(contents, contentType)
            {
                FileDownloadName = fileDownloadName,
                EnableRangeProcessing = enableRangeProcessing,
                LastModified = lastModified,
                EntityTag = entityTag,
            };

        /// <summary>
        /// Writes the specified <see cref="System.IO.Stream"/> to the response.
        /// <para>
        /// This supports range requests (<see cref="StatusCodes.Status206PartialContent"/> or
        /// <see cref="StatusCodes.Status416RangeNotSatisfiable"/> if the range is not satisfiable).
        /// </para>
        /// <para>
        /// This API is an alias for <see cref="Stream(Stream, string, string?, DateTimeOffset?, EntityTagHeaderValue?, bool)"/>.
        /// </para>
        /// </summary>
        /// <param name="fileStream">The <see cref="System.IO.Stream"/> with the contents of the file.</param>
        /// <param name="contentType">The Content-Type of the file.</param>
        /// <param name="fileDownloadName">The the file name to be used in the <c>Content-Disposition</c> header.</param>
        /// <param name="lastModified">The <see cref="DateTimeOffset"/> of when the file was last modified.
        /// Used to configure the <c>Last-Modified</c> response header and perform conditional range requests.</param>
        /// <param name="entityTag">The <see cref="EntityTagHeaderValue"/> to be configure the <c>ETag</c> response header
        /// and perform conditional requests.</param>
        /// <param name="enableRangeProcessing">Set to <c>true</c> to enable range requests processing.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        /// <remarks>
        /// The <paramref name="fileStream" /> parameter is disposed after the response is sent.
        /// </remarks>
#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
        public static HttpFileStream File(
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
                Stream fileStream,
            string? contentType = null,
            string? fileDownloadName = null,
            DateTimeOffset? lastModified = null,
            EntityTagHeaderValue? entityTag = null,
            bool enableRangeProcessing = false)
        {
            return new(fileStream, contentType)
            {
                LastModified = lastModified,
                EntityTag = entityTag,
                FileDownloadName = fileDownloadName,
                EnableRangeProcessing = enableRangeProcessing,
            };
        }

        /// <summary>
        /// Writes the specified <see cref="System.IO.Stream"/> to the response.
        /// <para>
        /// This supports range requests (<see cref="StatusCodes.Status206PartialContent"/> or
        /// <see cref="StatusCodes.Status416RangeNotSatisfiable"/> if the range is not satisfiable).
        /// </para>
        /// <para>
        /// This API is an alias for <see cref="File(Stream, string, string?, DateTimeOffset?, EntityTagHeaderValue?, bool)"/>.
        /// </para>
        /// </summary>
        /// <param name="stream">The <see cref="System.IO.Stream"/> to write to the response.</param>
        /// <param name="contentType">The <c>Content-Type</c> of the response. Defaults to <c>application/octet-stream</c>.</param>
        /// <param name="fileDownloadName">The the file name to be used in the <c>Content-Disposition</c> header.</param>
        /// <param name="lastModified">The <see cref="DateTimeOffset"/> of when the file was last modified.
        /// Used to configure the <c>Last-Modified</c> response header and perform conditional range requests.</param>
        /// <param name="entityTag">The <see cref="EntityTagHeaderValue"/> to be configure the <c>ETag</c> response header
        /// and perform conditional requests.</param>
        /// <param name="enableRangeProcessing">Set to <c>true</c> to enable range requests processing.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        /// <remarks>
        /// The <paramref name="stream" /> parameter is disposed after the response is sent.
        /// </remarks>
        public static HttpFileStream Stream(
            Stream stream,
            string? contentType = null,
            string? fileDownloadName = null,
            DateTimeOffset? lastModified = null,
            EntityTagHeaderValue? entityTag = null,
            bool enableRangeProcessing = false)
        {
            return new(stream, contentType)
            {
                LastModified = lastModified,
                EntityTag = entityTag,
                FileDownloadName = fileDownloadName,
                EnableRangeProcessing = enableRangeProcessing,
            };
        }

        /// <summary>
        /// Writes the contents of specified <see cref="System.IO.Pipelines.PipeReader"/> to the response.
        /// <para>
        /// This supports range requests (<see cref="StatusCodes.Status206PartialContent"/> or
        /// <see cref="StatusCodes.Status416RangeNotSatisfiable"/> if the range is not satisfiable).
        /// </para>
        /// </summary>
        /// <param name="pipeReader">The <see cref="System.IO.Pipelines.PipeReader"/> to write to the response.</param>
        /// <param name="contentType">The <c>Content-Type</c> of the response. Defaults to <c>application/octet-stream</c>.</param>
        /// <param name="fileDownloadName">The the file name to be used in the <c>Content-Disposition</c> header.</param>
        /// <param name="lastModified">The <see cref="DateTimeOffset"/> of when the file was last modified.
        /// Used to configure the <c>Last-Modified</c> response header and perform conditional range requests.</param>
        /// <param name="entityTag">The <see cref="EntityTagHeaderValue"/> to be configure the <c>ETag</c> response header
        /// and perform conditional requests.</param>
        /// <param name="enableRangeProcessing">Set to <c>true</c> to enable range requests processing.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        /// <remarks>
        /// The <paramref name="pipeReader" /> parameter is completed after the response is sent.
        /// </remarks>
#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
        public static HttpFileStream Stream(
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
            PipeReader pipeReader,
            string? contentType = null,
            string? fileDownloadName = null,
            DateTimeOffset? lastModified = null,
            EntityTagHeaderValue? entityTag = null,
            bool enableRangeProcessing = false)
        {
            return new(pipeReader.AsStream(), contentType)
            {
                LastModified = lastModified,
                EntityTag = entityTag,
                FileDownloadName = fileDownloadName,
                EnableRangeProcessing = enableRangeProcessing,
            };
        }

        /// <summary>
        /// Allows writing directly to the response body.
        /// <para>
        /// This supports range requests (<see cref="StatusCodes.Status206PartialContent"/> or
        /// <see cref="StatusCodes.Status416RangeNotSatisfiable"/> if the range is not satisfiable).
        /// </para>
        /// </summary>
        /// <param name="streamWriterCallback">The callback that allows users to write directly to the response body.</param>
        /// <param name="contentType">The <c>Content-Type</c> of the response. Defaults to <c>application/octet-stream</c>.</param>
        /// <param name="fileDownloadName">The the file name to be used in the <c>Content-Disposition</c> header.</param>
        /// <param name="lastModified">The <see cref="DateTimeOffset"/> of when the file was last modified.
        /// Used to configure the <c>Last-Modified</c> response header and perform conditional range requests.</param>
        /// <param name="entityTag">The <see cref="EntityTagHeaderValue"/> to be configure the <c>ETag</c> response header
        /// and perform conditional requests.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
        public static PushStream Stream(
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
            Func<Stream, Task> streamWriterCallback,
            string? contentType = null,
            string? fileDownloadName = null,
            DateTimeOffset? lastModified = null,
            EntityTagHeaderValue? entityTag = null)
        {
            return new(streamWriterCallback, contentType)
            {
                LastModified = lastModified,
                EntityTag = entityTag,
                FileDownloadName = fileDownloadName,
            };
        }

        /// <summary>
        /// Writes the file at the specified <paramref name="path"/> to the response.
        /// <para>
        /// This supports range requests (<see cref="StatusCodes.Status206PartialContent"/> or
        /// <see cref="StatusCodes.Status416RangeNotSatisfiable"/> if the range is not satisfiable).
        /// </para>
        /// </summary>
        /// <param name="path">The path to the file. When not rooted, resolves the path relative to <see cref="IWebHostEnvironment.WebRootFileProvider"/>.</param>
        /// <param name="contentType">The Content-Type of the file.</param>
        /// <param name="fileDownloadName">The suggested file name.</param>
        /// <param name="lastModified">The <see cref="DateTimeOffset"/> of when the file was last modified.</param>
        /// <param name="entityTag">The <see cref="EntityTagHeaderValue"/> associated with the file.</param>
        /// <param name="enableRangeProcessing">Set to <c>true</c> to enable range requests processing.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
        public static PhysicalFile PhysicalFile(
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
                string path,
            string? contentType = null,
            string? fileDownloadName = null,
            DateTimeOffset? lastModified = null,
            EntityTagHeaderValue? entityTag = null,
            bool enableRangeProcessing = false)
        {
            return new(path, contentType)
            {
                FileDownloadName = fileDownloadName,
                LastModified = lastModified,
                EntityTag = entityTag,
                EnableRangeProcessing = enableRangeProcessing,
            };
        }

        /// <summary>
        /// Writes the file at the specified <paramref name="path"/> to the response.
        /// <para>
        /// This supports range requests (<see cref="StatusCodes.Status206PartialContent"/> or
        /// <see cref="StatusCodes.Status416RangeNotSatisfiable"/> if the range is not satisfiable).
        /// </para>
        /// </summary>
        /// <param name="path">The path to the file. When not rooted, resolves the path relative to <see cref="IWebHostEnvironment.WebRootFileProvider"/>.</param>
        /// <param name="contentType">The Content-Type of the file.</param>
        /// <param name="fileDownloadName">The suggested file name.</param>
        /// <param name="lastModified">The <see cref="DateTimeOffset"/> of when the file was last modified.</param>
        /// <param name="entityTag">The <see cref="EntityTagHeaderValue"/> associated with the file.</param>
        /// <param name="enableRangeProcessing">Set to <c>true</c> to enable range requests processing.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
        public static VirtualFile VirtualFile(
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
                string path,
            string? contentType = null,
            string? fileDownloadName = null,
            DateTimeOffset? lastModified = null,
            EntityTagHeaderValue? entityTag = null,
            bool enableRangeProcessing = false)
        {
            return new(path, contentType)
            {
                FileDownloadName = fileDownloadName,
                LastModified = lastModified,
                EntityTag = entityTag,
                EnableRangeProcessing = enableRangeProcessing,
            };
        }

        /// <summary>
        /// Redirects to the specified <paramref name="url"/>.
        /// <list type="bullet">
        /// <item>When <paramref name="permanent"/> and <paramref name="preserveMethod"/> are set, sets the <see cref="StatusCodes.Status308PermanentRedirect"/> status code.</item>
        /// <item>When <paramref name="preserveMethod"/> is set, sets the <see cref="StatusCodes.Status307TemporaryRedirect"/> status code.</item>
        /// <item>When <paramref name="permanent"/> is set, sets the <see cref="StatusCodes.Status301MovedPermanently"/> status code.</item>
        /// <item>Otherwise, configures <see cref="StatusCodes.Status302Found"/>.</item>
        /// </list>
        /// </summary>
        /// <param name="url">The URL to redirect to.</param>
        /// <param name="permanent">Specifies whether the redirect should be permanent (301) or temporary (302).</param>
        /// <param name="preserveMethod">If set to true, make the temporary redirect (307) or permanent redirect (308) preserve the initial request method.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static Redirect Redirect(string url, bool permanent = false, bool preserveMethod = false)
            => new(url, permanent, preserveMethod);

        /// <summary>
        /// Redirects to the specified <paramref name="localUrl"/>.
        /// <list type="bullet">
        /// <item>When <paramref name="permanent"/> and <paramref name="preserveMethod"/> are set, sets the <see cref="StatusCodes.Status308PermanentRedirect"/> status code.</item>
        /// <item>When <paramref name="preserveMethod"/> is set, sets the <see cref="StatusCodes.Status307TemporaryRedirect"/> status code.</item>
        /// <item>When <paramref name="permanent"/> is set, sets the <see cref="StatusCodes.Status301MovedPermanently"/> status code.</item>
        /// <item>Otherwise, configures <see cref="StatusCodes.Status302Found"/>.</item>
        /// </list>
        /// </summary>
        /// <param name="localUrl">The local URL to redirect to.</param>
        /// <param name="permanent">Specifies whether the redirect should be permanent (301) or temporary (302).</param>
        /// <param name="preserveMethod">If set to true, make the temporary redirect (307) or permanent redirect (308) preserve the initial request method.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static Redirect LocalRedirect(string localUrl, bool permanent = false, bool preserveMethod = false)
            => new(localUrl, acceptLocalUrlOnly: true, permanent, preserveMethod);

        /// <summary>
        /// Redirects to the specified route.
        /// <list type="bullet">
        /// <item>When <paramref name="permanent"/> and <paramref name="preserveMethod"/> are set, sets the <see cref="StatusCodes.Status308PermanentRedirect"/> status code.</item>
        /// <item>When <paramref name="preserveMethod"/> is set, sets the <see cref="StatusCodes.Status307TemporaryRedirect"/> status code.</item>
        /// <item>When <paramref name="permanent"/> is set, sets the <see cref="StatusCodes.Status301MovedPermanently"/> status code.</item>
        /// <item>Otherwise, configures <see cref="StatusCodes.Status302Found"/>.</item>
        /// </list>
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="routeValues">The parameters for a route.</param>
        /// <param name="permanent">Specifies whether the redirect should be permanent (301) or temporary (302).</param>
        /// <param name="preserveMethod">If set to true, make the temporary redirect (307) or permanent redirect (308) preserve the initial request method.</param>
        /// <param name="fragment">The fragment to add to the URL.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static RedirectToRoute RedirectToRoute(string? routeName = null, object? routeValues = null, bool permanent = false, bool preserveMethod = false, string? fragment = null)
            => new(
                routeName: routeName,
                routeValues: routeValues,
                permanent: permanent,
                preserveMethod: preserveMethod,
                fragment: fragment);

        /// <summary>
        /// Creates a <see cref="Http.StatusCode"/> object by specifying a <paramref name="statusCode"/>.
        /// </summary>
        /// <param name="statusCode">The status code to set on the response.</param>
        /// <returns>The created <see cref="Http.StatusCode"/> object for the response.</returns>
        public static Status StatusCode(int statusCode)
            => ResultsCache.StatusCode(statusCode);

        /// <summary>
        /// Produces a <see cref="StatusCodes.Status404NotFound"/> response.
        /// </summary>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static NotFound NotFound() => ResultsCache.NotFound;

        /// <summary>
        /// Produces a <see cref="StatusCodes.Status404NotFound"/> response.
        /// </summary>
        /// <param name="value">The value to be included in the HTTP response body.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static NotFound<TValue> NotFound<TValue>(TValue? value) => new(value);

        /// <summary>
        /// Produces a <see cref="StatusCodes.Status401Unauthorized"/> response.
        /// </summary>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static Unauthorized Unauthorized() => ResultsCache.Unauthorized;

        /// <summary>
        /// Produces a <see cref="StatusCodes.Status400BadRequest"/> response.
        /// </summary>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static BadRequest BadRequest() => ResultsCache.BadRequest;

        /// <summary>
        /// Produces a <see cref="StatusCodes.Status400BadRequest"/> response.
        /// </summary>
        /// <param name="error">The value to be included in the HTTP response body.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static BadRequest<TValue> BadRequest<TValue>(TValue? error) => new(error);

        /// <summary>
        /// Produces a <see cref="StatusCodes.Status409Conflict"/> response.
        /// </summary>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static Conflict Conflict() => ResultsCache.Conflict;

        /// <summary>
        /// Produces a <see cref="StatusCodes.Status409Conflict"/> response.
        /// </summary>
        /// <param name="error">The value to be included in the HTTP response body.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static Conflict<TValue> Conflict<TValue>(TValue? error) => new(error);

        /// <summary>
        /// Produces a <see cref="StatusCodes.Status204NoContent"/> response.
        /// </summary>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static NoContent NoContent() => ResultsCache.NoContent;

        /// <summary>
        /// Produces a <see cref="StatusCodes.Status200OK"/> response.
        /// </summary>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static Ok Ok() => ResultsCache.Ok;

        /// <summary>
        /// Produces a <see cref="StatusCodes.Status200OK"/> response.
        /// </summary>
        /// <param name="value">The value to be included in the HTTP response body.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static Ok<TValue> Ok<TValue>(TValue? value) => new(value);

        /// <summary>
        /// Produces a <see cref="StatusCodes.Status422UnprocessableEntity"/> response.
        /// </summary>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static UnprocessableEntity UnprocessableEntity() => ResultsCache.UnprocessableEntity;

        /// <summary>
        /// Produces a <see cref="StatusCodes.Status422UnprocessableEntity"/> response.
        /// </summary>
        /// <param name="error">The value to be included in the HTTP response body.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static UnprocessableEntity<TValue> UnprocessableEntity<TValue>(TValue? error) => new(error);

        /// <summary>
        /// Produces a <see cref="ProblemDetails"/> response.
        /// </summary>
        /// <param name="statusCode">The value for <see cref="ProblemDetails.Status" />.</param>
        /// <param name="detail">The value for <see cref="ProblemDetails.Detail" />.</param>
        /// <param name="instance">The value for <see cref="ProblemDetails.Instance" />.</param>
        /// <param name="title">The value for <see cref="ProblemDetails.Title" />.</param>
        /// <param name="type">The value for <see cref="ProblemDetails.Type" />.</param>
        /// <param name="extensions">The value for <see cref="ProblemDetails.Extensions" />.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static Problem Problem(
            string? detail = null,
            string? instance = null,
            int? statusCode = null,
            string? title = null,
            string? type = null,
            IDictionary<string, object?>? extensions = null)
        {
            var problemDetails = new ProblemDetails
            {
                Detail = detail,
                Instance = instance,
                Status = statusCode,
                Title = title,
                Type = type,
            };

            if (extensions is not null)
            {
                foreach (var extension in extensions)
                {
                    problemDetails.Extensions.Add(extension);
                }
            }

            return new(problemDetails);
        }

        /// <summary>
        /// Produces a <see cref="ProblemDetails"/> response.
        /// </summary>
        /// <param name="problemDetails">The <see cref="ProblemDetails"/>  object to produce a response from.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static Problem Problem(ProblemDetails problemDetails)
        {
            return new(problemDetails);
        }

        /// <summary>
        /// Produces a <see cref="StatusCodes.Status400BadRequest"/> response
        /// with a <see cref="HttpValidationProblemDetails"/> value.
        /// </summary>
        /// <param name="errors">One or more validation errors.</param>
        /// <param name="detail">The value for <see cref="ProblemDetails.Detail" />.</param>
        /// <param name="instance">The value for <see cref="ProblemDetails.Instance" />.</param>
        /// <param name="statusCode">The status code.</param>
        /// <param name="title">The value for <see cref="ProblemDetails.Title" />. Defaults to "One or more validation errors occurred."</param>
        /// <param name="type">The value for <see cref="ProblemDetails.Type" />.</param>
        /// <param name="extensions">The value for <see cref="ProblemDetails.Extensions" />.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static Problem ValidationProblem(
            IDictionary<string, string[]> errors,
            string? detail = null,
            string? instance = null,
            int? statusCode = null,
            string? title = null,
            string? type = null,
            IDictionary<string, object?>? extensions = null)
        {
            var problemDetails = new HttpValidationProblemDetails(errors)
            {
                Detail = detail,
                Instance = instance,
                Type = type,
                Status = statusCode,
            };

            problemDetails.Title = title ?? problemDetails.Title;

            if (extensions is not null)
            {
                foreach (var extension in extensions)
                {
                    problemDetails.Extensions.Add(extension);
                }
            }

            return new(problemDetails);
        }

        /// <summary>
        /// Produces a <see cref="StatusCodes.Status201Created"/> response.
        /// </summary>
        /// <param name="uri">The URI at which the content has been created.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static Created Created(string uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            return new(uri);
        }

        /// <summary>
        /// Produces a <see cref="StatusCodes.Status201Created"/> response.
        /// </summary>
        /// <param name="uri">The URI at which the content has been created.</param>
        /// <param name="value">The value to be included in the HTTP response body.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static Created<TValue> Created<TValue>(string uri, TValue? value)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            return new(uri, value);
        }

        /// <summary>
        /// Produces a <see cref="StatusCodes.Status201Created"/> response.
        /// </summary>
        /// <param name="uri">The URI at which the content has been created.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static Created Created(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            return new(uri);
        }

        /// <summary>
        /// Produces a <see cref="StatusCodes.Status201Created"/> response.
        /// </summary>
        /// <param name="uri">The URI at which the content has been created.</param>
        /// <param name="value">The value to be included in the HTTP response body.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static Created<TValue> Created<TValue>(Uri uri, TValue? value)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            return new(uri, value);
        }

        /// <summary>
        /// Produces a <see cref="StatusCodes.Status201Created"/> response.
        /// </summary>
        /// <param name="routeName">The name of the route to use for generating the URL.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static CreatedAtRoute CreatedAtRoute(string? routeName = null, object? routeValues = null)
            => new(routeName, routeValues);

        /// <summary>
        /// Produces a <see cref="StatusCodes.Status201Created"/> response.
        /// </summary>
        /// <param name="routeName">The name of the route to use for generating the URL.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <param name="value">The value to be included in the HTTP response body.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static CreatedAtRoute<TValue> CreatedAtRoute<TValue>(TValue? value, string? routeName = null, object? routeValues = null)
            => new(routeName, routeValues, value);

        /// <summary>
        /// Produces a <see cref="StatusCodes.Status202Accepted"/> response.
        /// </summary>
        /// <param name="uri">The URI with the location at which the status of requested content can be monitored.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static Accepted Accepted(string uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            return new(uri);
        }

        /// <summary>
        /// Produces a <see cref="StatusCodes.Status202Accepted"/> response.
        /// </summary>
        /// <param name="uri">The URI with the location at which the status of requested content can be monitored.</param>
        /// <param name="value">The value to be included in the HTTP response body.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static Accepted<TValue> Accepted<TValue>(string uri, TValue? value)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            return new(uri, value);
        }

        /// <summary>
        /// Produces a <see cref="StatusCodes.Status202Accepted"/> response.
        /// </summary>
        /// <param name="uri">The URI with the location at which the status of requested content can be monitored.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static Accepted Accepted(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            return new(uri);
        }

        /// <summary>
        /// Produces a <see cref="StatusCodes.Status202Accepted"/> response.
        /// </summary>
        /// <param name="uri">The URI with the location at which the status of requested content can be monitored.</param>
        /// <param name="value">The value to be included in the HTTP response body.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static Accepted<TValue> Accepted<TValue>(Uri uri, TValue? value)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            return new(uri, value);
        }

        /// <summary>
        /// Produces a <see cref="StatusCodes.Status202Accepted"/> response.
        /// </summary>
        /// <param name="routeName">The name of the route to use for generating the URL.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static AcceptedAtRoute AcceptedAtRoute(string? routeName = null, object? routeValues = null)
            => new(routeName, routeValues);

        /// <summary>
        /// Produces a <see cref="StatusCodes.Status202Accepted"/> response.
        /// </summary>
        /// <param name="routeName">The name of the route to use for generating the URL.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <param name="value">The value to be included in the HTTP response body.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        public static AcceptedAtRoute<TValue> AcceptedAtRoute<TValue>(TValue? value, string? routeName = null, object? routeValues = null)
            => new(routeName, routeValues, value);
    }
}
