using System.Collections.Generic;
using Kentico.Xperience.Admin.Base.Forms;

namespace XperienceCommunity.FormValidation.UI;

/// <summary>
/// Validation rule definition used to display correct set of components.
/// </summary>
internal class ValidationRuleClientDefinition
{
	/// <summary>
	/// Gets or sets the name of the field the validation rule belongs to.
	/// </summary>
	public string CurrentFieldName { get; internal set; }

	/// <summary>
	/// <see cref="P:System.Type.FullName" /> of the underlying non-nullable type of the currently edited field.
	/// </summary>
	public string CurrentFieldTypeFullName { get; internal set; }

	/// <summary>Gets or sets the identifier.</summary>
	public string Identifier { get; set; }

	/// <summary>Gets the validation rule name.</summary>
	public string Name { get; internal set; }

	/// <summary>Gets or sets the validation rule description.</summary>
	public string Description { get; set; }

	/// <summary>
	/// Gets or sets components configuring the validation rule.
	/// </summary>
	public IEnumerable<IFormComponentClientProperties> ComponentsProperties { get; set; }
}