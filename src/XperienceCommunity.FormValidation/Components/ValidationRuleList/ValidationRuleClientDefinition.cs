using System.Collections.Generic;
using Kentico.Xperience.Admin.Base.Forms;

namespace XperienceCommunity.FormValidation.Components.ValidationRuleList;

internal class ValidationRuleClientDefinition
{
	public string CurrentFieldName { get; internal set; }
	public string CurrentFieldTypeFullName { get; internal set; }
	public string Identifier { get; set; }
	public string Name { get; internal set; }
	public string Description { get; set; }
	public IEnumerable<IFormComponentClientProperties> ComponentsProperties { get; set; }
}