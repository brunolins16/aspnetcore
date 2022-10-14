// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Http;

internal sealed class PropertyAsParameterInfo : ParameterInfo
{
    private readonly PropertyInfo _underlyingProperty;
    private readonly ParameterInfo? _constructionParameterInfo;
    private readonly Attribute? _parentSourceAttribute;

    private readonly NullabilityInfoContext _nullabilityContext;
    private NullabilityInfo? _nullabilityInfo;

    public PropertyAsParameterInfo(PropertyInfo propertyInfo, NullabilityInfoContext? nullabilityContext = null, Attribute? parentSourceAttribute = null)
    {
        Debug.Assert(null != propertyInfo);

        AttrsImpl = (ParameterAttributes)propertyInfo.Attributes;
        NameImpl = propertyInfo.Name;
        MemberImpl = propertyInfo;
        ClassImpl = propertyInfo.PropertyType;

        // It is not a real parameter in the delegate, so,
        // not defining a real position.
        PositionImpl = -1;

        _nullabilityContext = nullabilityContext ?? new NullabilityInfoContext();
        _underlyingProperty = propertyInfo;
        _parentSourceAttribute = parentSourceAttribute;
    }

    public PropertyAsParameterInfo(PropertyInfo property, ParameterInfo parameterInfo, NullabilityInfoContext? nullabilityContext = null, Attribute? parentSourceAttribute = null)
        : this(property, nullabilityContext, parentSourceAttribute)
    {
        _constructionParameterInfo = parameterInfo;
    }

    public override bool HasDefaultValue
        => _constructionParameterInfo is not null && _constructionParameterInfo.HasDefaultValue;
    public override object? DefaultValue
        => _constructionParameterInfo is not null ? _constructionParameterInfo.DefaultValue : null;
    public override int MetadataToken => _underlyingProperty.MetadataToken;
    public override object? RawDefaultValue
        => _constructionParameterInfo is not null ? _constructionParameterInfo.RawDefaultValue : null;

    /// <summary>
    /// Unwraps all parameters that contains <see cref="AsParametersAttribute"/> and
    /// creates a flat list merging the current parameters, not including the
    /// parametres that contain a <see cref="AsParametersAttribute"/>, and all additional
    /// parameters detected.
    /// </summary>
    /// <param name="parameters">List of parameters to be flattened.</param>
    /// <param name="cache">An instance of the method cache class.</param>
    /// <returns>Flat list of parameters.</returns>
    [UnconditionalSuppressMessage("Trimmer", "IL2075", Justification = "PropertyAsParameterInfo.Flatten requires unreferenced code.")]
    public static ReadOnlySpan<ParameterInfo> Flatten(ParameterInfo[] parameters, ParameterBindingMethodCache cache)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(cache);

        if (parameters.Length == 0)
        {
            return Array.Empty<ParameterInfo>();
        }

        List<ParameterInfo>? flattenedParameters = null;
        NullabilityInfoContext? nullabilityContext = null;

        for (var i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].Name is null)
            {
                throw new InvalidOperationException($"Encountered a parameter of type '{parameters[i].ParameterType}' without a name. Parameters must have a name.");
            }

            if (parameters[i].CustomAttributes.Any(a => a.AttributeType == typeof(AsParametersAttribute)))
            {
                // Initialize the list with all parameter already processed
                // to keep the same parameter ordering
                flattenedParameters ??= new(parameters[0..i]);
                nullabilityContext ??= new();

                var isNullable = Nullable.GetUnderlyingType(parameters[i].ParameterType) != null ||
                    nullabilityContext.Create(parameters[i])?.ReadState == NullabilityState.Nullable;

                if (isNullable)
                {
                    throw new InvalidOperationException($"The nullable type '{TypeNameHelper.GetTypeDisplayName(parameters[i].ParameterType, fullName: false)}' is not supported.");
                }

                var sourceAttribute = GetSourceAttribute(parameters[i]);
                var (constructor, constructorParameters) = cache.FindConstructor(parameters[i].ParameterType);
                if (constructor is not null && constructorParameters is { Length: > 0 })
                {
                    foreach (var constructorParameter in constructorParameters)
                    {
                        flattenedParameters.Add(
                            new PropertyAsParameterInfo(
                                constructorParameter.PropertyInfo,
                                constructorParameter.ParameterInfo,
                                nullabilityContext,
                                sourceAttribute));
                    }
                }
                else
                {
                    var properties = parameters[i].ParameterType.GetProperties();

                    foreach (var property in properties)
                    {
                        if (property.CanWrite)
                        {
                            flattenedParameters.Add(new PropertyAsParameterInfo(property, nullabilityContext, sourceAttribute));
                        }
                    }
                }
            }
            else if (flattenedParameters is not null)
            {
                flattenedParameters.Add(parameters[i]);
            }
        }

        return flattenedParameters is not null ? CollectionsMarshal.AsSpan(flattenedParameters) : parameters.AsSpan();
    }

    private static Attribute? GetSourceAttribute(ParameterInfo parameter)
    {
        var attributes = parameter.GetCustomAttributes().ToArray();
        for (var i = 0; i < attributes.Length; i++)
        {
            var attribute = attributes[i];

            if (attribute is IFromRouteMetadata ||
                attribute is IFromServiceMetadata ||
                attribute is IFromBodyMetadata)
            {
                throw new NotSupportedException("FromRoute / FromServices / FromBody not supported.");
            }
            else if (attribute is IFromQueryMetadata ||
                attribute is IFromHeaderMetadata ||
                attribute is IFromFormMetadata)
            {
                return attribute;
            }
        }

        return null;
    }

    public override object[] GetCustomAttributes(Type attributeType, bool inherit)
    {
        var attributes = _constructionParameterInfo?.GetCustomAttributes(attributeType, inherit);

        if (attributes == null || attributes is { Length: 0 })
        {
            attributes = _underlyingProperty.GetCustomAttributes(attributeType, inherit);
        }

        if (_parentSourceAttribute != null && attributeType.IsAssignableFrom(_parentSourceAttribute.GetType()))
        {
            if (attributes is { Length: 0 })
            {
                return new[] { _parentSourceAttribute };
            }

            var combinedAttributes = new Attribute[attributes.Length + 1];
            combinedAttributes[0] = _parentSourceAttribute;
            attributes.CopyTo(combinedAttributes, 1);
            return combinedAttributes;
        }

        return attributes;
    }

    public override object[] GetCustomAttributes(bool inherit)
    {
        var constructorAttributes = _constructionParameterInfo?.GetCustomAttributes(inherit);

        if (constructorAttributes == null || constructorAttributes is { Length: 0 })
        {
            return _underlyingProperty.GetCustomAttributes(inherit);
        }

        var propertyAttributes = _underlyingProperty.GetCustomAttributes(inherit);

        var constructorAttributesLength = constructorAttributes?.Length ?? 0;
        var propertyAttributesLength = propertyAttributes?.Length ?? 0;
        var requestSourceLength = _parentSourceAttribute == null ? 1 : 0;

        // Since the constructors attributes should take priority we will add them first,
        // as we usually call it as First() or FirstOrDefault() in the argument creation
        var mergedAttributes = new Attribute[constructorAttributesLength + propertyAttributesLength + requestSourceLength];

        if (_parentSourceAttribute != null)
        {
            mergedAttributes[0] = _parentSourceAttribute;
        }

        if (constructorAttributesLength > 0)
        {
            Array.Copy(constructorAttributes!, 0, mergedAttributes, requestSourceLength, constructorAttributesLength);
        }

        if (propertyAttributesLength > 0)
        {
            Array.Copy(propertyAttributes!, 0, mergedAttributes, constructorAttributesLength + requestSourceLength, propertyAttributesLength);
        }

        return mergedAttributes;
    }

    public override IList<CustomAttributeData> GetCustomAttributesData()
    {
        var attributes = new List<CustomAttributeData>(
            _constructionParameterInfo?.GetCustomAttributesData() ?? Array.Empty<CustomAttributeData>());
        attributes.AddRange(_underlyingProperty.GetCustomAttributesData());

        if (_parentSourceAttribute != null)
        {
            attributes.Insert(0, new AttributeDataWrapper(_parentSourceAttribute));
        }

        return attributes.AsReadOnly();
    }

    public override Type[] GetOptionalCustomModifiers()
        => _underlyingProperty.GetOptionalCustomModifiers();

    public override Type[] GetRequiredCustomModifiers()
        => _underlyingProperty.GetRequiredCustomModifiers();

    public override bool IsDefined(Type attributeType, bool inherit)
    {
        return (_constructionParameterInfo is not null && _constructionParameterInfo.IsDefined(attributeType, inherit)) ||
            _underlyingProperty.IsDefined(attributeType, inherit);
    }

    public new bool IsOptional => HasDefaultValue || NullabilityInfo.ReadState != NullabilityState.NotNull;

    public NullabilityInfo NullabilityInfo
        => _nullabilityInfo ??= _constructionParameterInfo is not null ?
        _nullabilityContext.Create(_constructionParameterInfo) :
        _nullabilityContext.Create(_underlyingProperty);
}

internal sealed class AttributeDataWrapper : CustomAttributeData
{
    public AttributeDataWrapper(Attribute attribute)
    {
        Type type = attribute.GetType();

        Constructor = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)[0];
        ConstructorArguments = Array.Empty<CustomAttributeTypedArgument>();
        NamedArguments = Array.Empty<CustomAttributeNamedArgument>();
    }

    public override ConstructorInfo Constructor { get; }
    public override IList<CustomAttributeTypedArgument> ConstructorArguments { get; }
    public override IList<CustomAttributeNamedArgument> NamedArguments { get; }
}
