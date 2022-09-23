// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Http.Abstractions.Tests;

public class ProblemDetailsJsonConverterTest
{
    private static JsonSerializerOptions JsonSerializerOptions => new JsonOptions().SerializerOptions;

    [Fact]
    public void Read_ThrowsIfJsonIsIncomplete()
    {
        // Arrange
        var json = "{";

        // Act & Assert
        var ex = Record.Exception(() =>
        {
            JsonSerializer.Deserialize<ProblemDetails>(json, JsonSerializerOptions);
        });
        Assert.IsAssignableFrom<JsonException>(ex);
    }

    [Fact]
    public void Read_UsingJsonSerializerWorks()
    {
        // Arrange
        var type = "https://tools.ietf.org/html/rfc9110#section-15.5.5";
        var title = "Not found";
        var status = 404;
        var detail = "Product not found";
        var instance = "http://example.com/products/14";
        var traceId = "|37dd3dd5-4a9619f953c40a16.";
        var json = $"{{\"type\":\"{type}\",\"title\":\"{title}\",\"status\":{status},\"detail\":\"{detail}\", \"instance\":\"{instance}\",\"traceId\":\"{traceId}\"}}";

        // Act
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(json, JsonSerializerOptions);

        // Assert
        Assert.NotNull(problemDetails);
        Assert.Equal(type, problemDetails!.Type);
        Assert.Equal(title, problemDetails.Title);
        Assert.Equal(status, problemDetails.Status);
        Assert.Equal(instance, problemDetails.Instance);
        Assert.Equal(detail, problemDetails.Detail);
        Assert.Collection(
            problemDetails.Extensions,
            kvp =>
            {
                Assert.Equal("traceId", kvp.Key);
                Assert.Equal(traceId, kvp.Value?.ToString());
            });
    }

    [Fact]
    public void Read_UsingJsonSerializerWorks_WithSomeMissingValues_Works()
    {
        // Arrange
        var type = "https://tools.ietf.org/html/rfc9110#section-15.5.5";
        var title = "Not found";
        var status = 404;
        var traceId = "|37dd3dd5-4a9619f953c40a16.";
        var json = $"{{\"type\":\"{type}\",\"title\":\"{title}\",\"status\":{status},\"traceId\":\"{traceId}\"}}";

        // Act
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(json, JsonSerializerOptions);

        // Assert
        Assert.NotNull(problemDetails);
        Assert.Equal(type, problemDetails.Type);
        Assert.Equal(title, problemDetails.Title);
        Assert.Equal(status, problemDetails.Status);
        Assert.Collection(
            problemDetails.Extensions,
            kvp =>
            {
                Assert.Equal("traceId", kvp.Key);
                Assert.Equal(traceId, kvp.Value?.ToString());
            });
    }

    [Fact]
    public void Write_Works()
    {
        // Arrange
        var traceId = "|37dd3dd5-4a9619f953c40a16.";
        var value = new ProblemDetails
        {
            Title = "Not found",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
            Status = 404,
            Detail = "Product not found",
            Instance = "http://example.com/products/14",
            Extensions =
                {
                    { "traceId", traceId },
                    { "some-data", new[] { "value1", "value2" } }
                }
        };
        var expected = $"{{\"type\":\"{JsonEncodedText.Encode(value.Type)}\",\"title\":\"{value.Title}\",\"status\":{value.Status},\"detail\":\"{value.Detail}\",\"instance\":\"{JsonEncodedText.Encode(value.Instance)}\",\"traceId\":\"{traceId}\",\"some-data\":[\"value1\",\"value2\"]}}";
        using var stream = new MemoryStream();

        // Act
        JsonSerializer.Serialize(stream, value, JsonSerializerOptions);

        // Assert
        var actual = Encoding.UTF8.GetString(stream.ToArray());
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Write_WithSomeMissingContent_Works()
    {
        // Arrange
        var value = new ProblemDetails
        {
            Title = "Not found",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
            Status = 404,
        };
        var expected = $"{{\"type\":\"{JsonEncodedText.Encode(value.Type)}\",\"title\":\"{value.Title}\",\"status\":{value.Status}}}";
        using var stream = new MemoryStream();

        // Act
        JsonSerializer.Serialize(stream, value, JsonSerializerOptions);

        // Assert
        var actual = Encoding.UTF8.GetString(stream.ToArray());
        Assert.Equal(expected, actual);
    }
}
