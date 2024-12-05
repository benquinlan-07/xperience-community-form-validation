using System.Collections.Generic;
using Kentico.Xperience.Admin.Base.Forms;

namespace XperienceCommunity.FormValidation.UI;

internal sealed class CreateValidationRuleResult
{
	/// <summary>Validation rule to be added to the client.</summary>
	public ValidationRuleConfiguration Rule { get; init; }

	/// <summary>
	/// Description of the rule shown in the listing of validation rules.
	/// </summary>
	public string Description { get; init; }

	/// <summary>
	/// Returns true if binding of form data to validation rule editing components is valid.
	/// </summary>
	public bool IsValid { get; init; } = true;

	/// <summary>
	/// Client component properties of the rule that was attempted to be created.
	/// If bound components are not valid, they are returned to the client to be re-rendered with validation failure messages.
	/// </summary>
	public IEnumerable<IFormComponentClientProperties> ComponentsProperties { get; init; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Kentico.Xperience.Admin.Base.Internal.CreateValidationRuleResult" /> class.
	/// </summary>
	internal CreateValidationRuleResult()
	{
	}
}