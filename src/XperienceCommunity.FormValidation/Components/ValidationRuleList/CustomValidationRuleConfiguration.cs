using Kentico.Forms.Web.Mvc;

namespace XperienceCommunity.FormValidation.Components.ValidationRuleList;

public sealed class CustomValidationRuleConfiguration
{
    public string Identifier { get; set; }
    public string ValidationRuleIdentifier { get; set; }
    public ValidationRule RuleValues { get; set; }
}