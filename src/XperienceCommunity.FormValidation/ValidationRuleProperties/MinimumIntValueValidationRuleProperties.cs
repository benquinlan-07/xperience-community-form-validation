using System.ComponentModel.DataAnnotations;
using CMS.Core;
using CMS.Helpers;
using Kentico.Forms.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace XperienceCommunity.FormValidation.ValidationRuleProperties;

public class MinimumIntValueValidationRuleProperties : Kentico.Xperience.Admin.Base.Forms.ValidationRuleProperties, IFormValidationRuleProperties<MinimumIntValueValidationRule>
{
    [NumberInputComponent(Label = "{$kentico.formbuilder.validationrule.minimumvalue.minimumvalue.label$}")]
    [Required]
    public int MinimumValue { get; set; } = 1;

    public override string GetDescriptionText(ILocalizationService localizationService)
    {
        return ResHelper.GetStringFormat("kentico.formbuilder.validationrule.minimumvalue.title", (object)this.MinimumValue);
    }
}