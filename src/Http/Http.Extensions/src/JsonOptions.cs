// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

#nullable enable

namespace Microsoft.AspNetCore.Http.Json;

/// <summary>
/// Options to configure JSON serialization settings for <see cref="HttpRequestJsonExtensions"/>
/// and <see cref="HttpResponseJsonExtensions"/>.
/// </summary>
public class JsonOptions
{
    internal static readonly JsonSerializerOptions DefaultSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        // Web defaults don't use the relex JSON escaping encoder.
        //
        // Because these options are for producing content that is written directly to the request
        // (and not embedded in an HTML page for example), we can use UnsafeRelaxedJsonEscaping.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,

        // The JsonSerializerOptions.GetTypeInfo method is called directly and needs a defined resolver
        // setting the default resolver (reflection-based) but the user can overwrite it directly or calling
        // .AddContext<TContext>()
#pragma warning disable IL2026 // Disabling IL2026 since JsonTrimmability.IsTrimmable is a feature switch and adding RequiresUnreferencedCode or UnconditionalSuppressMessage is not right since we would like to have it reported to users is the default value is changed 
        TypeInfoResolver = JsonTrimmability.IsTrimmable ? null : new DefaultJsonTypeInfoResolver()
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
    };

    // Use a copy so the defaults are not modified.
    /// <summary>
    /// Gets the <see cref="JsonSerializerOptions"/>.
    /// </summary>
    public JsonSerializerOptions SerializerOptions { get; } = new JsonSerializerOptions(DefaultSerializerOptions);
}
