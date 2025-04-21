using System.ComponentModel.DataAnnotations;
using CMS.Core;
using CMS.Helpers;
using Kentico.Forms.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace XperienceCommunity.FormValidation.ValidationRuleProperties;

public class MaximumLengthValidationRuleProperties : Kentico.Xperience.Admin.Base.Forms.ValidationRuleProperties, IFormValidationRuleProperties<MaximumLengthValidationRule>
{
    [NumberInputComponent(Label = "{$kentico.formbuilder.validationrule.maximumlength.maximumlength.label$}")]
    [Required]
    public int MaximumLength { get; set; } = 100;

    public override string GetDescriptionText(ILocalizationService localizationService)
    {
        return ResHelper.GetStringFormat("kentico.formbuilder.validationrule.maximumlength.title", (object)this.MaximumLength);
    }
}