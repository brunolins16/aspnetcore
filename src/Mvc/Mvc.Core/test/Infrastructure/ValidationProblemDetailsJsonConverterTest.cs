// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using System.Text.Json;

namespace Microsoft.AspNetCore.Mvc.Infrastructure;

public class ValidationProblemDetailsJsonConverterTest
{
    private static JsonSerializerOptions JsonSerializerOptions => new JsonOptions().JsonSerializerOptions;

    [Fact]
    public void Read_Works()
    {
        // Arrange
        var type = "https://tools.ietf.org/html/rfc9110#section-15.5.5";
        var title = "Not found";
        var status = 404;
        var detail = "Product not found";
        var instance = "http://example.com/products/14";
        var traceId = "|37dd3dd5-4a9619f953c40a16.";
        var json = $"{{\"type\":\"{type}\",\"title\":\"{title}\",\"status\":{status},\"detail\":\"{detail}\", \"instance\":\"{instance}\",\"traceId\":\"{traceId}\"," +
            "\"errors\":{\"key0\":[\"error0\"],\"key1\":[\"error1\",\"error2\"]}}";

        // Act
        var problemDetails = JsonSerializer.Deserialize<ValidationProblemDetails>(json, JsonSerializerOptions);

        Assert.Equal(type, problemDetails.Type);
        Assert.Equal(title, problemDetails.Title);
        Assert.Equal(status, problemDetails.Status);
        Assert.Equal(instance, problemDetails.Instance);
        Assert.Equal(detail, problemDetails.Detail);
        Assert.Collection(
            problemDetails.Extensions,
            kvp =>
            {
                Assert.Equal("traceId", kvp.Key);
                Assert.Equal(traceId, kvp.Value.ToString());
            });
        Assert.Collection(
            problemDetails.Errors.OrderBy(kvp => kvp.Key),
            kvp =>
            {
                Assert.Equal("key0", kvp.Key);
                Assert.Equal(new[] { "error0" }, kvp.Value);
            },
            kvp =>
            {
                Assert.Equal("key1", kvp.Key);
                Assert.Equal(new[] { "error1", "error2" }, kvp.Value);
            });
    }

    [Fact]
    public void Read_WithSomeMissingValues_Works()
    {
        // Arrange
        var type = "https://tools.ietf.org/html/rfc9110#section-15.5.5";
        var title = "Not found";
        var status = 404;
        var traceId = "|37dd3dd5-4a9619f953c40a16.";
        var json = $"{{\"type\":\"{type}\",\"title\":\"{title}\",\"status\":{status},\"traceId\":\"{traceId}\"," +
            "\"errors\":{\"key0\":[\"error0\"],\"key1\":[\"error1\",\"error2\"]}}";

        // Act
        var problemDetails = JsonSerializer.Deserialize<ValidationProblemDetails>(json, JsonSerializerOptions);

        Assert.Equal(type, problemDetails.Type);
        Assert.Equal(title, problemDetails.Title);
        Assert.Equal(status, problemDetails.Status);
        Assert.Collection(
            problemDetails.Extensions,
            kvp =>
            {
                Assert.Equal("traceId", kvp.Key);
                Assert.Equal(traceId, kvp.Value.ToString());
            });
        Assert.Collection(
            problemDetails.Errors.OrderBy(kvp => kvp.Key),
            kvp =>
            {
                Assert.Equal("key0", kvp.Key);
                Assert.Equal(new[] { "error0" }, kvp.Value);
            },
            kvp =>
            {
                Assert.Equal("key1", kvp.Key);
                Assert.Equal(new[] { "error1", "error2" }, kvp.Value);
            });
    }

    [Fact]
    public void ReadUsingJsonSerializerWorks()
    {
        // Arrange
        var type = "https://tools.ietf.org/html/rfc9110#section-15.5.5";
        var title = "Not found";
        var status = 404;
        var traceId = "|37dd3dd5-4a9619f953c40a16.";
        var json = $"{{\"type\":\"{type}\",\"title\":\"{title}\",\"status\":{status},\"traceId\":\"{traceId}\"," +
            "\"errors\":{\"key0\":[\"error0\"],\"key1\":[\"error1\",\"error2\"]}}";

        // Act
        var problemDetails = JsonSerializer.Deserialize<ValidationProblemDetails>(json, JsonSerializerOptions);

        Assert.Equal(type, problemDetails.Type);
        Assert.Equal(title, problemDetails.Title);
        Assert.Equal(status, problemDetails.Status);
        Assert.Collection(
            problemDetails.Extensions,
            kvp =>
            {
                Assert.Equal("traceId", kvp.Key);
                Assert.Equal(traceId, kvp.Value.ToString());
            });
        Assert.Collection(
            problemDetails.Errors.OrderBy(kvp => kvp.Key),
            kvp =>
            {
                Assert.Equal("key0", kvp.Key);
                Assert.Equal(new[] { "error0" }, kvp.Value);
            },
            kvp =>
            {
                Assert.Equal("key1", kvp.Key);
                Assert.Equal(new[] { "error1", "error2" }, kvp.Value);
            });
    }

    [Fact]
    public void WriteWorks()
    {
        var problemDetails = new ValidationProblemDetails(new Dictionary<string, string[]>() { { "Property", new string[] { "error0" } } })
        {
            Title = "One or more validation errors occurred.",
            Status = 400
        };

        // Act
        var json = JsonSerializer.Serialize(problemDetails, options: null);

        var expectedJSON = $"{{\"Title\":\"{problemDetails.Title}\",\"Status\":{problemDetails.Status}," +
            "\"Errors\":{\"Property\":[\"error0\"]}}";
        Assert.NotNull(json);
        Assert.Equal(expectedJSON, json);
    }

    [Fact]
    public void WriteUsingJsonSerializerOptionsWorks()
    {
        var errors = new Dictionary<string, string[]>()
        {
            { "Property",  new string[]{ "error0" } },
            { "TwoWords",  new string[]{ "error1" } },
            { "TopLevelProperty.PropertyName",  new string[]{ "error2" } },
        };
        var problemDetails = new ValidationProblemDetails(errors)
        {
            Title = "One or more validation errors occurred.",
            Status = 400
        };

        // Act
        var options = new JsonOptions().JsonSerializerOptions;
        options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;

        var json = JsonSerializer.Serialize(problemDetails, options);

        var expectedJSON = $"{{\"title\":\"{problemDetails.Title}\",\"status\":{problemDetails.Status}," +
            "\"errors\":{\"property\":[\"error0\"],\"twoWords\":[\"error1\"],\"topLevelProperty.PropertyName\":[\"error2\"]}}";
        Assert.NotNull(json);
        Assert.Equal(expectedJSON, json);
    }
}
