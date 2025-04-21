using XperienceCommunity.FormValidation.Components.ValidationRuleList;

namespace XperienceCommunity.FormValidation.UI;

internal sealed class CreateValidationRuleResult
{
	/// <summary>Validation rule to be added to the client.</summary>
	public CustomValidationRuleConfiguration Rule { get; init; }

	/// <summary>
	/// Description of the rule shown in the listing of validation rules.
	/// </summary>
	public string Description { get; init; }

    public bool IsValid { get; set; }
    public object ComponentsProperties { get; set; }
}