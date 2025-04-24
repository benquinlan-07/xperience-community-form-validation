using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CMS.Core;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.OnlineForms;
using Kentico.Forms.Web.Mvc;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Base.Forms.Internal;
using Kentico.Xperience.Admin.DigitalMarketing.UIPages;
using XperienceCommunity.FormValidation.Components.ValidationRuleList;
using XperienceCommunity.FormValidation.Extensions;
using XperienceCommunity.FormValidation.JsonConverters;
using XperienceCommunity.FormValidation.Models;
using XperienceCommunity.FormValidation.UI;
using TextInputComponent = Kentico.Xperience.Admin.Base.Forms.TextInputComponent;

[assembly: PageExtender(typeof(CustomPageExtender))]
[assembly: Kentico.Xperience.Admin.Base.Forms.RegisterFormComponent("RacingQueensland.AdminComponents.ValidationRuleListComponent", typeof(ValidationRuleListComponent), "Custom ValidationRuleListComponent")]

namespace XperienceCommunity.FormValidation.UI;

public class CustomPageExtender : PageExtender<FormBuilderTab>
{
	const string ValidationRulesPropertyName = "CustomValidationRuleConfigurations";
	const string RequiredErrorMessage = "RequiredErrorMessage";

	private readonly ILocalizationService _localizationService;
	private readonly IFormItemCollectionProvider _formItemCollectionProvider;
    private readonly IValidationRuleDefinitionProvider _validationRuleDefinitionProvider;
    private readonly IInfoProvider<FormFieldValidationInfo> _formFieldValidationInfoProvider;
    private readonly IFormComponentDefinitionProvider _formComponentDefinitionProvider;

    private readonly JsonSerializerOptions _validationRulesSerializerOptions;

    public CustomPageExtender(ILocalizationService localizationService, 
        IFormItemCollectionProvider formItemCollectionProvider,
        IValidationRuleDefinitionProvider validationRuleDefinitionProvider,
        IInfoProvider<FormFieldValidationInfo> formFieldValidationInfoProvider,
        IFormComponentDefinitionProvider formComponentDefinitionProvider)
	{
		_localizationService = localizationService;
		_formItemCollectionProvider = formItemCollectionProvider;
        _validationRuleDefinitionProvider = validationRuleDefinitionProvider;
        _formFieldValidationInfoProvider = formFieldValidationInfoProvider;
        _formComponentDefinitionProvider = formComponentDefinitionProvider;
        CommandExecute.After += CommandExecute_After;
        _validationRulesSerializerOptions = new JsonSerializerOptions() { Converters = { new ValidationRuleConfigurationConverter(_validationRuleDefinitionProvider) } };

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
    }

    [PageCommand(Permission = "Update")]
	public async Task<ICommandResponse> CreateValidationRule(CreateValidationRuleArgs args)
    {
        var validationRule = _validationRuleDefinitionProvider.Get(args.Identifier);
		var validationRuleInstance = (ValidationRule)Activator.CreateInstance(validationRule.ValidationRuleType);
		foreach (var key in args.FormData.Keys)
		{
			var property = validationRule.ValidationRuleType.GetProperty(key);
			if (property != null)
				property.SetValue(validationRuleInstance, args.FormData[key].Deserialize(property.PropertyType));
		}

        return ResponseFrom(new CreateValidationRuleResult
        {
            Rule = new CustomValidationRuleConfiguration
            {
                Identifier = Guid.NewGuid().ToString(),
                ValidationRuleIdentifier = args.Identifier,
                RuleValues = validationRuleInstance
            },
            Description = !string.IsNullOrWhiteSpace(validationRuleInstance?.ErrorMessage) ? validationRuleInstance.ErrorMessage : validationRule.Description,
            IsValid = true,
            ComponentsProperties = null
        });
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

		// Try and get the form validation rules from the validation class if available
        var formFieldValidation = _formFieldValidationInfoProvider.Get()
            .WhereEquals(nameof(FormFieldValidationInfo.FormFieldValidationFormGUID), this.EditedForm.FormGUID)
            .WhereEquals(nameof(FormFieldValidationInfo.FormFieldValidationFieldGUID), Guid.Parse(parameters.Identifier))
            .FirstOrDefault();

        CustomValidationRuleConfiguration[] validationRules = null;
        if (formFieldValidation != null)
            validationRules = JsonSerializer.Deserialize<CustomValidationRuleConfiguration[]>(formFieldValidation.FormFieldValidationRules, _validationRulesSerializerOptions);
        if (validationRules == null)
            validationRules = [];

        // Ensure we were able to parse all the rules
        validationRules = validationRules.Where(x => x.RuleValues != null).ToArray();

        var formFields = response.Result.ToList();
		formFields.Add(new FormCategoryClientProperties() { Title = "Validation", ComponentName = "@kentico/xperience-admin-base/CollapsibleLabelCategory" });

        var requiredErrorField = new TextInputClientProperties()
        {
            WatermarkText = ResHelper.GetString("general.requiresvalue"),
            ComponentName = new TextInputComponent(_localizationService).ClientComponentName,
            Name = RequiredErrorMessage,
            Label = "Required error message",
            Value = formFieldValidation?.FormFieldValidationRequiredError ?? "",
            AdditionalActions = [],
            ValidationRules = []
        };
        formFields.Add(requiredErrorField);

        var formComponentDefinition = _formComponentDefinitionProvider.Get(parameters.TypeIdentifier);
        if (formComponentDefinition != null)
        {
            var validationField = GetValidationElement(fieldName, formComponentDefinition.ValueType, validationRules).Result;
            validationField.Name = ValidationRulesPropertyName;
            validationField.Label = "Validation rules";
            formFields.Add(validationField);
        }

		pageCommandEventArgs.CommandResponse = new CommandResponse<IEnumerable<IFormItemClientProperties>>(formFields);
    }

    private void OnAfterValidateComponents(PageCommandEventArgs pageCommandEventArgs)
    {
        var parameters = pageCommandEventArgs.CommandParameters as ComponentPropertiesFormSubmissionCommandArgs;
        var response = pageCommandEventArgs.CommandResponse as ICommandResponse<ComponentPropertiesFormSubmissionCommandResult>;
        if (response == null || parameters == null || !response.Result.IsValid)
            return;

        if (parameters.FormData.ContainsKey(ValidationRulesPropertyName) && !response.Result.FormItems.OfType<ValidationRuleClientProperties>().Any())
        {
            var validationRules = parameters.FormData[ValidationRulesPropertyName].Deserialize<CustomValidationRuleConfiguration[]>(_validationRulesSerializerOptions);
            var requiredErrorMessage = parameters.FormData.ContainsKey(RequiredErrorMessage) ? parameters.FormData[RequiredErrorMessage].Deserialize<string>() : null;

            var fieldGuid = Guid.Parse(parameters.Identifier);

            // Attempt to load existing validation
            var formFieldValidation = _formFieldValidationInfoProvider.Get()
                .WhereEquals(nameof(FormFieldValidationInfo.FormFieldValidationFormGUID), EditedForm.FormGUID)
                .WhereEquals(nameof(FormFieldValidationInfo.FormFieldValidationFieldGUID), fieldGuid)
                .FirstOrDefault();

            // Initialise new validation if not exists
            if (formFieldValidation == null)
            {
                formFieldValidation = new FormFieldValidationInfo()
                {
                    FormFieldValidationFormGUID = EditedForm.FormGUID,
                    FormFieldValidationFieldGUID = fieldGuid
                };
            }

            // Set the validation data
            formFieldValidation.FormFieldValidationRequiredError = requiredErrorMessage;
            formFieldValidation.FormFieldValidationRules = JsonSerializer.Serialize(validationRules, _validationRulesSerializerOptions);

            // Save the validation changes
            _formFieldValidationInfoProvider.Set(formFieldValidation);
        }
    }

    private async Task<IFormComponentClientProperties> GetValidationElement(string currentFieldName, Type fieldType, CustomValidationRuleConfiguration[] validationRules)
	{
		var formInfo = new FormInfo(DataClassInfo.ClassFormDefinition);
		var fields = formInfo.GetFields(true, true);

		var component = new ValidationRuleListComponent(_localizationService, _formItemCollectionProvider, _validationRuleDefinitionProvider);
		component.SetRuleDefinitionsByType(currentFieldName, fieldType, fields);
        component.SetValue(validationRules);
		return await component.GetClientProperties();
    }
}