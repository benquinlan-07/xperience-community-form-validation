using System.ComponentModel.DataAnnotations;
using CMS.Core;
using CMS.Helpers;
using Kentico.Forms.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace XperienceCommunity.FormValidation.ValidationRuleProperties;

public class RegularExpressionValidationRuleProperties : Kentico.Xperience.Admin.Base.Forms.ValidationRuleProperties, IFormValidationRuleProperties<RegularExpressionValidationRule>
{
    [TextInputComponent(Label = "{$kentico.formbuilder.validationrule.regularexpression.regularexpression.label$}")]
    [Required]
    public string RegularExpression { get; set; }

    public override string GetDescriptionText(ILocalizationService localizationService)
    {
        return ResHelper.GetStringFormat("kentico.formbuilder.validationrule.regularexpression.title", (object)this.RegularExpression);
    }
}