using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CMS.Core;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.OnlineForms;
using Kentico.Forms.Web.Mvc;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.DigitalMarketing.UIPages;
using XperienceCommunity.FormValidation.Extensions;
using XperienceCommunity.FormValidation.UI;
using RegisterFormValidationRuleAttribute = Kentico.Xperience.Admin.Base.Forms.RegisterFormValidationRuleAttribute;
using ValidationRuleConfiguration = Kentico.Xperience.Admin.Base.Forms.ValidationRuleConfiguration;

[assembly: PageExtender(typeof(CustomPageExtender))]
[assembly: Kentico.Xperience.Admin.Base.Forms.RegisterFormComponent("RacingQueensland.AdminComponents.ValidationRuleListComponent", typeof(ValidationRuleListComponent), "Custom ValidationRuleListComponent")]

namespace XperienceCommunity.FormValidation.UI;

public class CustomPageExtender : PageExtender<FormBuilderTab>
{
	const string ValidationRulesPropertyName = "ValidationRules";

	private readonly ILocalizationService _localizationService;
	private readonly Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider _formItemCollectionProvider;
	private readonly IFormDataBinder _formDataBinder;
	private readonly IValidationRuleActivator _validationRuleActivator;

	public CustomPageExtender(ILocalizationService localizationService, Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider formItemCollectionProvider, IFormDataBinder formDataBinder,
		IValidationRuleActivator validationRuleActivator)
	{
		_localizationService = localizationService;
		_formItemCollectionProvider = formItemCollectionProvider;
		_formDataBinder = formDataBinder;
		_validationRuleActivator = validationRuleActivator;
		CommandExecute.After += CommandExecute_After;
	}

	[PageCommand(Permission = "Update")]
	public async Task<ICommandResponse> CreateValidationRule(CreateValidationRuleArgs args)
	{
		var validationRulePropertiesType = AssemblyDiscoveryHelper.GetAssemblies(true).SelectMany(a => a.GetCustomAttributes(typeof(RegisterFormValidationRuleAttribute), true))
			.Cast<RegisterFormValidationRuleAttribute>()
			.Select(x => new ValidationRuleDefinition(x.Identifier, x.MarkedType, x.Name, x.Description))
			.Where(x => x.Identifier == args.Identifier)
			.Select(x => x.GetPropertiesType())
			.First();

		ValidationRuleProperties validationRuleProperties = (ValidationRuleProperties)Activator.CreateInstance(validationRulePropertiesType);
		var components = (await _formItemCollectionProvider.GetFormItems(validationRuleProperties, CancellationToken.None)).OfType<IFormComponent>().ToArray();
		foreach (var component in components)
		{
			var formDataValue = args.FormData[component.Name];
			component.SetObjectValue(formDataValue.Deserialize(component.ValueType));
			validationRulePropertiesType.GetProperty(component.Name).SetValue(validationRuleProperties, component.GetObjectValue());
		}

		return ResponseFrom(new CreateValidationRuleResult()
		{
			Rule = new ValidationRuleConfiguration()
			{
				RuleValues = validationRuleProperties,
				ValidationRuleIdentifier = args.Identifier
			},
			Description = string.IsNullOrEmpty(validationRuleProperties.ErrorMessage) ? validationRuleProperties.GetDescriptionText(_localizationService) : validationRuleProperties.ErrorMessage
		});
	}

	public override async Task ConfigurePage()
	{
		await base.ConfigurePage();
	}

	private void CommandExecute_After(object sender, PageCommandEventArgs e)
	{
		switch (e.CommandName)
		{
			case nameof(FormBuilderTab.GetFormItems):
				OnAfterGetFormItems(e);
				break;
			case nameof(FormBuilderTab.ValidateComponents):
				OnAfterValidateComponents(e);
				break;
		}

		//if (e.CommandName == nameof(FormBuilderTab.SaveMapping))
		//{
		//	//var formInfo = new FormInfo(DataClassInfo.ClassFormDefinition);
		//	//DataClassInfo.ClassFormDefinition = formInfo.GetXmlDefinition();
		//	//DataClassInfo.Update();
		//}
	}

	private void OnAfterValidateComponents(PageCommandEventArgs pageCommandEventArgs)
	{
		var parameters = pageCommandEventArgs.CommandParameters as ComponentPropertiesFormSubmissionCommandArgs;
		var response = pageCommandEventArgs.CommandResponse as ICommandResponse<ComponentPropertiesFormSubmissionCommandResult>;
		if (response == null || parameters == null)
			return;

		if (parameters.FormData.ContainsKey(ValidationRulesPropertyName) && !response.Result.FormItems.OfType<ValidationRuleClientProperties>().Any())
		{
			var fieldName = parameters.FormData.ContainsKey("Name") ? parameters.FormData["Name"].ToString() : null;
			var validationRules = parameters.FormData[ValidationRulesPropertyName].Deserialize<ValidationRuleConfiguration[]>(new JsonSerializerOptions() { Converters = { new ValidationRuleConfigurationConverter() } });

			var actualRules = new List<Kentico.Forms.Web.Mvc.ValidationRuleConfiguration>();
			foreach (var validationRule in validationRules)
			{
				var rule = _validationRuleActivator.CreateValidationRule(validationRule.ValidationRuleIdentifier);
				actualRules.Add(new Kentico.Forms.Web.Mvc.ValidationRuleConfiguration(validationRule.ValidationRuleIdentifier, rule));
			}

			var formItems = response.Result.FormItems.ToList();
			var validation = GetValidationElement(fieldName, typeof(string)).Result;
			validation.Name = ValidationRulesPropertyName;
			validation.Label = "Validation rules";
			validation.Value = actualRules;
			formItems.Add(new FormCategoryClientProperties() { Title = "Validation", ComponentName = "@kentico/xperience-admin-base/CollapsibleLabelCategory" });
			formItems.Add(validation);

			// Update the result
			var updatedResult = new ComponentPropertiesFormSubmissionCommandResult()
			{
				IsValid = response.Result.IsValid,
				FormItems = formItems
			};
			pageCommandEventArgs.CommandResponse = new CommandResponse<ComponentPropertiesFormSubmissionCommandResult>(updatedResult);
		}
	}

	private void OnAfterGetFormItems(PageCommandEventArgs pageCommandEventArgs)
	{
		var parameters = pageCommandEventArgs.CommandParameters as ComponentPropertiesGetFormItemsCommandArgs;
		var response = pageCommandEventArgs.CommandResponse as ICommandResponse<IEnumerable<IFormItemClientProperties>>;
		if (response == null || parameters == null)
			return;

		const string fieldNameProperty = "Name";
		var fieldName = parameters.FormData.ContainsKey(fieldNameProperty)
			? parameters.FormData[fieldNameProperty].ToString()
			: parameters.FormData.ContainsKey(fieldNameProperty.ToLower())
				? parameters.FormData[fieldNameProperty.ToCamelCase()].ToString()
				: null;

		var validation = GetValidationElement(fieldName, typeof(string)).Result;
		validation.Name = ValidationRulesPropertyName;
		validation.Label = "Validation rules";

		validation.Value = parameters.FormData.ContainsKey(nameof(Kentico.Forms.Web.Mvc.FormComponentProperties.ValidationRuleConfigurations))
			 ? parameters.FormData[nameof(Kentico.Forms.Web.Mvc.FormComponentProperties.ValidationRuleConfigurations)].Deserialize<Kentico.Forms.Web.Mvc.ValidationRuleConfiguration[]>(new JsonSerializerOptions() { Converters = { new ValidationRuleConfigurationConverter() } })
			 : parameters.FormData.ContainsKey(nameof(Kentico.Forms.Web.Mvc.FormComponentProperties.ValidationRuleConfigurations).ToLower())
				 ? parameters.FormData[nameof(Kentico.Forms.Web.Mvc.FormComponentProperties.ValidationRuleConfigurations).ToCamelCase()].Deserialize<Kentico.Forms.Web.Mvc.ValidationRuleConfiguration[]>(new JsonSerializerOptions() { Converters = { new ValidationRuleConfigurationConverter() } })
				 : Array.Empty<Kentico.Forms.Web.Mvc.ValidationRuleConfiguration>();
		var formFields = response.Result.ToList();
		formFields.Add(new FormCategoryClientProperties() { Title = "Validation", ComponentName = "@kentico/xperience-admin-base/CollapsibleLabelCategory" });
		formFields.Add(validation);

		pageCommandEventArgs.CommandResponse = new CommandResponse<IEnumerable<IFormItemClientProperties>>(formFields);
	}

	private BizFormInfo mEditedForm;
	private BizFormInfo EditedForm
	{
		get
		{
			return mEditedForm ??= AbstractInfo<BizFormInfo, IBizFormInfoProvider>.Provider.Get(Page.ObjectId);
		}
	}

	private DataClassInfo mDataClassInfo;
	private DataClassInfo DataClassInfo
	{
		get
		{
			return mDataClassInfo ??= DataClassInfoProviderBase<DataClassInfoProvider>.GetDataClassInfo(EditedForm.FormClassID);
		}
	}

	private async Task<IFormComponentClientProperties> GetValidationElement(string currentFieldName, Type fieldType)
	{
		var formInfo = new FormInfo(DataClassInfo.ClassFormDefinition);
		var fields = formInfo.GetFields(true, true);

		var component = new ValidationRuleListComponent(_localizationService, _formItemCollectionProvider);
		component.SetRuleDefinitionsByType(currentFieldName, fieldType, fields);
		return await component.GetClientProperties();
	}

	///// <summary>
	///// Returns sub form for editing component validation rules.
	///// </summary>
	//public async Task<SubForm> GetValidationForm(
	//	GetFieldFormArguments arguments,
	//	string formComponentIdentifier)
	//{
	//	IFormComponent editingFormComponentComponent = (IFormComponent)null;
	//	if (formComponentIdentifier != null)
	//	{
	//		FormComponentDefinition componentDefinition = this.definitionProvider.Get(formComponentIdentifier);
	//		if (componentDefinition != null)
	//			editingFormComponentComponent = this.formComponentActivator.CreateComponent(componentDefinition.ComponentType, arguments.FormField, true);
	//	}
	//	ValidationSettingsModel model = new ValidationSettingsModel()
	//	{
	//		ValidationRules = (IEnumerable<ValidationRuleConfiguration>)ValidationRuleConfigurationsXmlSerializer.Instance.Deserialize(arguments.FormField.ValidationRuleConfigurationsXmlData)
	//	};
	//	List<IFormComponent> componentsForModel = await this.GetFormComponentsForModel((object)model);
	//	SubForm validationForm = new SubForm("ValidationSettingsModel", this.localizationService.GetString("base.fieldeditor.validation"), componentsForModel);
	//	var component = validationForm.GetComponent<ValidationRuleListComponent2>("ValidationRules");

	//	List<FormFieldInfo> fieldsForFormContext = this.GetFormFieldsForFormContext(arguments);
	//	string fieldName = FieldEditorSubFormProvider.GetFieldName(arguments);
	//	Type valueType = editingFormComponentComponent?.ValueType;
	//	List<FormFieldInfo> allFields = fieldsForFormContext;
	//	if (await component.SetRuleDefinitionsByType(fieldName, valueType, allFields) == 0)
	//		return (SubForm)null;
	//	await this.BindSubform(validationForm, (object)model, (IDictionary<string, Dictionary<string, JsonElement>>)arguments.SubFormValues, arguments.SkipValidation);
	//	return validationForm;
	//}
}