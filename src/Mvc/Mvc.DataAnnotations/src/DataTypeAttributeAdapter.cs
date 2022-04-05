// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;

namespace Microsoft.AspNetCore.Mvc.DataAnnotations;

/// <summary>
/// A validation adapter that is used to map <see cref="DataTypeAttribute"/>'s to a single client side validation
/// rule.
/// </summary>
internal class DataTypeAttributeAdapter : AttributeAdapterBase<DataTypeAttribute>
{
    public DataTypeAttributeAdapter(DataTypeAttribute attribute, string ruleName, IStringLocalizer? stringLocalizer)
        : base(attribute, stringLocalizer)
    {
        if (string.IsNullOrEmpty(ruleName))
        {
            throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(ruleName));
        }

        RuleName = ruleName;
    }

    public string RuleName { get; }

    public override void AddValidation(ClientModelValidationContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, RuleName, GetErrorMessage(context));
    }

    /// <inheritdoc/>
    public override string GetErrorMessage(ModelValidationContextBase validationContext)
    {
        if (validationContext == null)
        {
            throw new ArgumentNullException(nameof(validationContext));
        }

        var displayName = validationContext.HasApiValidationBehavior ?
            validationContext.ModelMetadata.GetValidationModelNameOrDisplayName() :
            validationContext.ModelMetadata.GetDisplayName();

        return GetErrorMessage(
            validationContext.ModelMetadata,
            displayName,
            Attribute.GetDataTypeName());
    }
}
