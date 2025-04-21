using CMS.Core;
using CMS.Helpers;
using EmailValidationRule = Kentico.Forms.Web.Mvc.EmailValidationRule;

namespace XperienceCommunity.FormValidation.ValidationRuleProperties;

internal class EmailValidationRuleProperties : Kentico.Xperience.Admin.Base.Forms.ValidationRuleProperties, IFormValidationRuleProperties<EmailValidationRule>
{
    internal const string IDENTIFIER = "Kentico.Email";

    public override string GetDescriptionText(ILocalizationService localizationService)
    {
        return ResHelper.GetString("kentico.formbuilder.validationrule.email.title");
    }
}