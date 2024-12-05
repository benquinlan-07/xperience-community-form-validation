using System.Collections.Generic;
using Kentico.Forms.Web.Mvc;

namespace XperienceCommunity.FormValidation.UI;

/// <summary>
/// Represents client properties of a <see cref="T:Kentico.Xperience.Admin.Base.Forms.ValidationRuleListComponent" />.
/// </summary>
internal sealed class ValidationRuleListClientProperties : Kentico.Xperience.Admin.Base.Forms.FormComponentClientProperties<IEnumerable<ValidationRuleConfiguration>>
{
	/// <summary>
	/// Gets or sets collection of rule descriptions which is displayed.
	/// </summary>
	public IDictionary<string, string> RulesDescriptions { get; set; }

	/// <summary>
	/// Gets or sets all definitions based on form component type.
	/// </summary>
	public IEnumerable<ValidationRuleClientDefinition> RuleDefinitions { get; set; }
}