using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CMS.Core;
using CMS.FormEngine;
using Kentico.Xperience.Admin.Base.Forms;
using ValidationRuleConfiguration = Kentico.Forms.Web.Mvc.ValidationRuleConfiguration;

namespace XperienceCommunity.FormValidation.UI;

/// <summary>Represents Validation rule list form component.</summary>
[ComponentAttribute(typeof(ValidationRuleListComponentAttribute))]
internal sealed class ValidationRuleListComponent : Kentico.Xperience.Admin.Base.Forms.FormComponent<ValidationRuleListClientProperties, IEnumerable<ValidationRuleConfiguration>>
{
	/// <summary>
	/// Represents the <see cref="T:Kentico.Xperience.Admin.Base.Forms.ValidationRuleListComponent" /> identifier.
	/// </summary>
	public const string IDENTIFIER = "Kentico.Administration.ValidationRuleList";
	private readonly ILocalizationService localizationService;
	private readonly Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider formItemCollectionProvider;
	private List<ValidationRuleClientDefinition> ruleClientDefinitions = new List<ValidationRuleClientDefinition>();

	/// <inheritdoc />
	public override string ClientComponentName
	{
		get => "@kentico/xperience-admin-base/ValidationRuleList";
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Kentico.Xperience.Admin.Base.Forms.ValidationRuleListComponent" /> class.
	/// The component is specially designed for the form field editor.
	/// </summary>
	/// <param name="localizationService">The localization service.</param>
	/// <param name="formItemCollectionProvider">The form item collection provider.</param>
	/// <remarks>
	/// It works with <see cref="T:Kentico.Xperience.Admin.Base.IFieldEditorFormProvider" /> that sets and filters the definition rules
	/// available on the component.
	/// </remarks>
	public ValidationRuleListComponent(
		ILocalizationService localizationService,
		Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider formItemCollectionProvider)
	{
		this.localizationService = localizationService;
		this.formItemCollectionProvider = formItemCollectionProvider;
	}

	/// <summary>
	/// Gets the <see cref="T:System.Collections.Generic.IEnumerable`1" /> value of the component.
	/// The value is filtered to only return rules valid for the available <see cref="P:Kentico.Xperience.Admin.Base.Forms.ValidationRuleListClientProperties.RuleDefinitions" />.
	/// </summary>
	/// <remarks>
	/// The value is filtered because on client the user may switch between data types and editing components which can change the <see cref="P:Kentico.Xperience.Admin.Base.Forms.ValidationRuleListClientProperties.RuleDefinitions" />
	/// Any possible validation rules are not removed from the <see cref="P:Kentico.Xperience.Admin.Base.Forms.IFormComponentClientProperties.Value" /> because data type and components may be switched back and forth without
	/// saving the form field, so original stored rules should be displayed in that case. Until the form is saved, which will use this method to retrieve rule configurations for the form field.
	/// </remarks>
	public override IEnumerable<Kentico.Forms.Web.Mvc.ValidationRuleConfiguration> GetValue()
	{
		IEnumerable<Kentico.Forms.Web.Mvc.ValidationRuleConfiguration> source = base.GetValue();
		IEnumerable<string> validIdentifiers = ruleClientDefinitions.Select<ValidationRuleClientDefinition, string>((Func<ValidationRuleClientDefinition, string>)(def => def.Identifier));
		Func<Kentico.Forms.Web.Mvc.ValidationRuleConfiguration, bool> predicate = val => validIdentifiers.Contains<string>(val.Identifier);
		return source.Where(predicate);
	}

	/// <summary>
	/// Sets the rule definitions available to the component filtered by the <paramref name="fieldComponentType" />.
	/// </summary>
	/// <returns>Number of the set rule definitions.</returns>
	/// <remarks>
	/// When changing editing component on field editor the available validation rules should be compatible with
	/// form components <c>{TValue}</c> of <see cref="T:Kentico.Xperience.Admin.Base.Forms.FormComponent`2" />.
	/// </remarks>
	internal async Task<int> SetRuleDefinitionsByType(
		string currentFieldName,
		Type fieldComponentType,
		List<FormFieldInfo> allFields)
	{

		var validationRuleDefinitions = AssemblyDiscoveryHelper.GetAssemblies(true).SelectMany<Assembly, object>(
				(Func<Assembly, IEnumerable<object>>)(a =>
					a.GetCustomAttributes(typeof(RegisterFormValidationRuleAttribute), true)))
			.Cast<RegisterFormValidationRuleAttribute>()
			.Select(x => new ValidationRuleDefinition(x.Identifier, x.MarkedType, x.Name, x.Description))
			.ToList();

		List<ValidationRuleClientDefinition> ruleData = new List<ValidationRuleClientDefinition>();
		string currentFieldTypeFullName = null;
		if ((object)fieldComponentType != null)
		{
			Type type = Nullable.GetUnderlyingType(fieldComponentType);
			if ((object)type == null)
				type = fieldComponentType;
			currentFieldTypeFullName = type.FullName;
		}
		//FieldEditorFormContext formContext = new FieldEditorFormContext()
		//{
		//	FormFields = (IList<FormFieldDescriptor>)allFields.Select<FormFieldInfo, FormFieldDescriptor>((Func<FormFieldInfo, FormFieldDescriptor>)(x => new FormFieldDescriptor(x.Name, DataTypeManager.GetDataType(TypeEnum.Field, x.DataType).Type, x.DataType, x.Visible))).ToList<FormFieldDescriptor>(),
		//	CurrentFieldName = currentFieldName,
		//	CurrentFieldTypeFullName = currentFieldTypeFullName
		//};
		foreach (ValidationRuleDefinition ruleDef in validationRuleDefinitions)
		{
			IEnumerable<IFormComponent> components = (await formItemCollectionProvider.GetFormItems(Activator.CreateInstance(ruleDef.GetPropertiesType()), CancellationToken.None)).OfType<IFormComponent>();
			//foreach (IFormComponent formComponent in components)
			//	await formComponent.BindContext((IFormContext)formContext);
			List<ValidationRuleClientDefinition> clientDefinitionList = ruleData;
			ValidationRuleClientDefinition clientDefinition1 = new ValidationRuleClientDefinition();
			clientDefinition1.CurrentFieldName = currentFieldName;
			clientDefinition1.CurrentFieldTypeFullName = currentFieldTypeFullName;
			clientDefinition1.Identifier = ruleDef.Identifier;
			clientDefinition1.Name = localizationService.LocalizeString(ruleDef.Name);
			clientDefinition1.Description = localizationService.LocalizeString(ruleDef.Description);
			ValidationRuleClientDefinition clientDefinition2 = clientDefinition1;
			clientDefinition2.ComponentsProperties = await components.GetClientProperties();
			clientDefinitionList.Add(clientDefinition1);
			clientDefinitionList = null;
			clientDefinition2 = null;
			clientDefinition1 = null;
			components = null;
		}
		ruleClientDefinitions = ruleData;
		int count = ruleData.Count;
		ruleData = null;
		currentFieldTypeFullName = null;
		return count;
	}

	/// <inheritdoc />
	protected override Task ConfigureClientProperties(
		ValidationRuleListClientProperties clientProperties)
	{
		clientProperties.RuleDefinitions = ruleClientDefinitions;
		clientProperties.RulesDescriptions = GetRuleDescriptions(clientProperties);
		return base.ConfigureClientProperties(clientProperties);
	}

	private IDictionary<string, string> GetRuleDescriptions(
		ValidationRuleListClientProperties clientProperties)
	{
		return clientProperties.Value == null ? new Dictionary<string, string>() : (IDictionary<string, string>)clientProperties.Value.ToDictionary<Kentico.Forms.Web.Mvc.ValidationRuleConfiguration, string, string>((Func<Kentico.Forms.Web.Mvc.ValidationRuleConfiguration, string>)(cfg => cfg.Identifier), (Func<Kentico.Forms.Web.Mvc.ValidationRuleConfiguration, string>)(cfg => !string.IsNullOrEmpty(cfg.ValidationRule.ErrorMessage) ? cfg.ValidationRule.ErrorMessage : cfg.ValidationRule.ErrorMessage));
	}
}