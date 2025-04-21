using CMS.Core;
using CMS.Helpers;
using Kentico.Forms.Web.Mvc;

namespace XperienceCommunity.FormValidation.ValidationRuleProperties
{
    internal class BoolIsSetValidationRuleProperties : Kentico.Xperience.Admin.Base.Forms.ValidationRuleProperties, IFormValidationRuleProperties<BoolIsSetValidationRule>
    {
        public override string GetDescriptionText(ILocalizationService localizationService)
        {
            return ResHelper.GetString("kentico.formbuilder.validationrule.boolisset.title");
        }
    }
}
