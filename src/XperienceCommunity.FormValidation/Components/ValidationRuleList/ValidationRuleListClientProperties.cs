using System.Collections.Generic;
using Kentico.Xperience.Admin.Base.Forms;

namespace XperienceCommunity.FormValidation.Components.ValidationRuleList;

/// <summary>
/// Represents client properties of a <see cref="T:Kentico.Xperience.Admin.Base.Forms.ValidationRuleListComponent" />.
/// </summary>
internal sealed class ValidationRuleListClientProperties : FormComponentClientProperties<IEnumerable<CustomValidationRuleConfiguration>>
{
	public IDictionary<string, string> RulesDescriptions { get; set; }
	public List<ValidationRuleClientDefinition> RuleDefinitions { get; set; }
}