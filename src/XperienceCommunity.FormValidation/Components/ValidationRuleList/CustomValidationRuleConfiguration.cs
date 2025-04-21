using System.Text.Json.Serialization;
using Kentico.Forms.Web.Mvc;
using XperienceCommunity.FormValidation.JsonConverters;

namespace XperienceCommunity.FormValidation.Components.ValidationRuleList;

[JsonConverter(typeof(ValidationRuleConfigurationConverter))]
public sealed class CustomValidationRuleConfiguration
{
    public string Identifier { get; set; }
    public string ValidationRuleIdentifier { get; set; }
    public ValidationRule RuleValues { get; set; }
}