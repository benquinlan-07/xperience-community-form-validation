using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CMS;
using CMS.Core;
using CMS.FormEngine;
using Kentico.Forms.Web.Mvc;
using Kentico.Xperience.Admin.Base.Forms;
using XperienceCommunity.FormValidation.ValidationRuleProperties;

namespace XperienceCommunity.FormValidation.Components.ValidationRuleList;

[ComponentAttribute(typeof(ValidationRuleListComponentAttribute))]
internal sealed class ValidationRuleListComponent : Kentico.Xperience.Admin.Base.Forms.FormComponent<ValidationRuleListClientProperties, IEnumerable<CustomValidationRuleConfiguration>>
{
	public const string Identifier = "Kentico.Administration.ValidationRuleList";

	private readonly ILocalizationService _localizationService;
	private readonly Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider _formItemCollectionProvider;
    private readonly IValidationRuleDefinitionProvider _validationRuleDefinitionProvider;
    private List<ValidationRuleClientDefinition> _ruleClientDefinitions = new List<ValidationRuleClientDefinition>();

	public override string ClientComponentName => "@kentico/xperience-admin-base/ValidationRuleList";

	public ValidationRuleListComponent(ILocalizationService localizationService,
		Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider formItemCollectionProvider,
        IValidationRuleDefinitionProvider validationRuleDefinitionProvider)
	{
		_localizationService = localizationService;
		_formItemCollectionProvider = formItemCollectionProvider;
        _validationRuleDefinitionProvider = validationRuleDefinitionProvider;
    }

	public override IEnumerable<CustomValidationRuleConfiguration> GetValue()
	{
		IEnumerable<CustomValidationRuleConfiguration> source = base.GetValue();
		IEnumerable<string> validIdentifiers = _ruleClientDefinitions.Select<ValidationRuleClientDefinition, string>((Func<ValidationRuleClientDefinition, string>)(def => def.Identifier));
		Func<CustomValidationRuleConfiguration, bool> predicate = val => validIdentifiers.Contains<string>(val.ValidationRuleIdentifier);
		return source.Where(predicate);
	}

    internal async Task<int> SetRuleDefinitionsByType(
        string currentFieldName,
        Type fieldComponentType,
        List<FormFieldInfo> allFields)
    {
        var allValidationRuleDefinitions = _validationRuleDefinitionProvider.GetAll().ToArray();
        var validationRuleDefinitions = allValidationRuleDefinitions
            .Where(x => x.ValidatedDataType == fieldComponentType || Nullable.GetUnderlyingType(fieldComponentType) == x.ValidatedDataType)
            .ToArray();

        List<ValidationRuleClientDefinition> ruleData = new List<ValidationRuleClientDefinition>();
        string currentFieldTypeFullName = null;
        if ((object)fieldComponentType != null)
        {
            Type type = Nullable.GetUnderlyingType(fieldComponentType);
            if ((object)type == null)
                type = fieldComponentType;
            currentFieldTypeFullName = type.FullName;
        }

        foreach (Kentico.Forms.Web.Mvc.ValidationRuleDefinition ruleDef in validationRuleDefinitions)
        {
            var components = await GetFormComponents(ruleDef.ValidationRuleType);
            if (!components.Any())
            {
                var genericInterfaceType = typeof(IFormValidationRuleProperties<>).MakeGenericType(ruleDef.ValidationRuleType);
                var customPropertiesType = GetFormValidationRulePropertyTypes().FirstOrDefault(x => genericInterfaceType.IsAssignableFrom(x));

                if (customPropertiesType != null)
                    components = await GetFormComponents(customPropertiesType);
            }

            var clientDefinition = new ValidationRuleClientDefinition
            {
                CurrentFieldName = currentFieldName,
                CurrentFieldTypeFullName = currentFieldTypeFullName,
                Identifier = ruleDef.Identifier,
                Name = _localizationService.LocalizeString(ruleDef.Name),
                Description = _localizationService.LocalizeString(ruleDef.Description),
                ComponentsProperties = await components.GetClientProperties()
            };
            ruleData.Add(clientDefinition);
        }
        _ruleClientDefinitions = ruleData;
        return ruleData.Count;
    }

    private async Task<IFormComponent[]> GetFormComponents(Type type)
    {
        var instance = Activator.CreateInstance(type);
        var formItems = await _formItemCollectionProvider.GetFormItems(instance, CancellationToken.None);
        return formItems
            .OfType<IFormComponent>()
            .ToArray();
    }

    private Type[] _formValidationRulePropertyTypes;
    private Type[] GetFormValidationRulePropertyTypes()
    {
        if (_formValidationRulePropertyTypes != null)
            return _formValidationRulePropertyTypes;

        var interfaceType = typeof(IFormValidationRuleProperties);

        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => x.GetCustomAttribute<AssemblyDiscoverableAttribute>() != null)
            .ToArray();

        var customPropertiesTypes = assemblies
            .SelectMany(x => x.GetTypes().Where(y => interfaceType.IsAssignableFrom(y)))
            .ToArray();

        return _formValidationRulePropertyTypes = customPropertiesTypes;
    }

	protected override Task ConfigureClientProperties(
		ValidationRuleListClientProperties clientProperties)
	{
		clientProperties.RuleDefinitions = _ruleClientDefinitions;
		clientProperties.RulesDescriptions = GetRuleDescriptions(clientProperties);
		return base.ConfigureClientProperties(clientProperties);
	}

	private IDictionary<string, string> GetRuleDescriptions(
		ValidationRuleListClientProperties clientProperties)
	{
		return clientProperties.Value == null 
            ? new Dictionary<string, string>() 
            : clientProperties.Value.ToDictionary(cfg => cfg.Identifier, cfg => cfg.RuleValues.ErrorMessage);
	}
}