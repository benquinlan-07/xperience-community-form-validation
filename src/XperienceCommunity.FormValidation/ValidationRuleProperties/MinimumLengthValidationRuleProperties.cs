using System.ComponentModel.DataAnnotations;
using CMS.Core;
using CMS.Helpers;
using Kentico.Forms.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace XperienceCommunity.FormValidation.ValidationRuleProperties;

public class MinimumLengthValidationRuleProperties : Kentico.Xperience.Admin.Base.Forms.ValidationRuleProperties, IFormValidationRuleProperties<MinimumLengthValidationRule>
{
    [NumberInputComponent(Label = "{$kentico.formbuilder.validationrule.minimumlength.minimumlength.label$}")]
    [Required]
    public int MinimumLength { get; set; } = 1;

    public override string GetDescriptionText(ILocalizationService localizationService)
    {
        return ResHelper.GetStringFormat("kentico.formbuilder.validationrule.minimumlength.title", (object)this.MinimumLength);
    }
}