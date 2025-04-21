using System.ComponentModel.DataAnnotations;
using CMS.Core;
using CMS.Helpers;
using Kentico.Forms.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace XperienceCommunity.FormValidation.ValidationRuleProperties;

public class MaximumIntValueValidationRuleProperties : Kentico.Xperience.Admin.Base.Forms.ValidationRuleProperties, IFormValidationRuleProperties<MaximumIntValueValidationRule>
{
    [NumberInputComponent(Label = "{$kentico.formbuilder.validationrule.maximumvalue.maximumvalue.label$}")]
    [Required]
    public int MaximumValue { get; set; } = 100;

    public override string GetDescriptionText(ILocalizationService localizationService)
    {
        return ResHelper.GetStringFormat("kentico.formbuilder.validationrule.maximumvalue.title", (object)this.MaximumValue);
    }
}