// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;

namespace Microsoft.AspNetCore.Mvc.DataAnnotations;

internal sealed class MaxLengthAttributeAdapter : AttributeAdapterBase<MaxLengthAttribute>
{
    private readonly string _max;

    public MaxLengthAttributeAdapter(MaxLengthAttribute attribute, IStringLocalizer? stringLocalizer)
        : base(attribute, stringLocalizer)
    {
        _max = Attribute.Length.ToString(CultureInfo.InvariantCulture);
    }

    public override void AddValidation(ClientModelValidationContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-maxlength", GetErrorMessage(context));
        MergeAttribute(context.Attributes, "data-val-maxlength-max", _max);
    }

    /// <inheritdoc />
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
            Attribute.Length);
    }
}
