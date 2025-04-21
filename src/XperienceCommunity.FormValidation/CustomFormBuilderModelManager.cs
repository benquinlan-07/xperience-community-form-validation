using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using CMS.Core;
using CMS.DataEngine;
using CMS.DataEngine.Internal;
using CMS.Helpers;
using Kentico.Forms.Web.Mvc;
using Microsoft.Extensions.DependencyInjection;
using XperienceCommunity.FormValidation.Components.ValidationRuleList;
using XperienceCommunity.FormValidation.JsonConverters;
using XperienceCommunity.FormValidation.Models;

namespace XperienceCommunity.FormValidation
{
    public class CustomFormComponentValidator : IFormComponentValidator
    {
        private readonly IFormComponentValidator _kenticoFormComponentValidator;
        private readonly IInfoProvider<FormFieldValidationInfo> _formFieldValidationInfoProvider;

        public CustomFormComponentValidator([FromKeyedServices("kentico")] IFormComponentValidator kenticoFormComponentValidator,
            IInfoProvider<FormFieldValidationInfo> formFieldValidationInfoProvider)
        {
            _kenticoFormComponentValidator = kenticoFormComponentValidator;
            _formFieldValidationInfoProvider = formFieldValidationInfoProvider;
        }

        public IEnumerable<string> ValidateComponentValue(FormComponent component)
        {
            var validationErrors = _kenticoFormComponentValidator.ValidateComponentValue(component).ToList();

            var formFieldValidationInfo = GetFormFieldValidation(component);
            if (formFieldValidationInfo == null)
                return validationErrors;

            var defaultRequiredError = ResHelper.GetString("general.requiresvalue");

            if (validationErrors.Contains(defaultRequiredError) && !string.IsNullOrWhiteSpace(formFieldValidationInfo.FormFieldValidationRequiredError))
            {
                validationErrors.Remove(defaultRequiredError);
                validationErrors.Add(formFieldValidationInfo.FormFieldValidationRequiredError);
            }

            var validationRules = ParseValidationRules(formFieldValidationInfo);

            var value = component.GetObjectValue();
            foreach (var config in validationRules)
            {
                if (!config.RuleValues.IsValueValid(value))
                {
                    validationErrors.Add(config.RuleValues.ErrorMessage);
                }
            }

            return validationErrors;
        }

        private FormFieldValidationInfo GetFormFieldValidation(FormComponent component)
        {
            var formFieldValidationInfo = _formFieldValidationInfoProvider.Get()
                .WhereEquals(nameof(FormFieldValidationInfo.FormFieldValidationFieldGUID), component.BaseProperties.Guid)
                .FirstOrDefault();

            return formFieldValidationInfo;
        }

        public IEnumerable<string> ValidateCompareToFieldRules(string formName, FormComponent component, List<FormComponent> allComponents)
        {
            var validationErrors = _kenticoFormComponentValidator.ValidateCompareToFieldRules(formName, component, allComponents).ToList();

            var formFieldValidationInfo = GetFormFieldValidation(component);
            if (formFieldValidationInfo == null)
                return validationErrors;

            var validationRules = ParseValidationRules(formFieldValidationInfo);

            foreach (var validationRule in validationRules.Select(x => x.RuleValues).Where((rule => rule.GetType().FindTypeByGenericDefinition(typeof(CompareToFieldValidationRule<>)) != null)))
            {
                var dependeeFieldGuidProperty = validationRule.GetType().GetProperty("DependeeFieldGuid");
                var setDependeeFieldValueMethod = validationRule.GetType().GetMethod("SetDependeeFieldValue");

                if (dependeeFieldGuidProperty == null || setDependeeFieldValueMethod == null)
                    continue;

                var objectValue1 = component.GetObjectValue();
                var formComponent = allComponents.Find((c => c.BaseProperties.Guid.Equals(dependeeFieldGuidProperty.GetValue(validationRule))));
                if (formComponent == null)
                    throw new InvalidOperationException($"Form '{formName}' contains component '{component.Name}' having validation rule '{validationRule.Title}' depending on missing field.");
                
                var fieldRule = ResHelper.LocalizeString(validationRule.ErrorMessage);
                var objectValue2 = formComponent.GetObjectValue();
                if (objectValue2 != null)
                {
                    setDependeeFieldValueMethod.Invoke(validationRule, new [] { objectValue2 });
                    if (!validationRule.IsValueValid(objectValue1))
                        validationErrors.Add(fieldRule);
                }
                else
                {
                    validationErrors.Add(fieldRule);
                }
            }

            return validationErrors;
        }

        private CustomValidationRuleConfiguration[] ParseValidationRules(FormFieldValidationInfo formFieldValidationInfo)
        {
            if (string.IsNullOrWhiteSpace(formFieldValidationInfo?.FormFieldValidationRules))
                return Array.Empty<CustomValidationRuleConfiguration>();

            var validationRuleDefinitionProvider = Service.Resolve<IValidationRuleDefinitionProvider>();
            var validationRules = JsonSerializer.Deserialize<CustomValidationRuleConfiguration[]>(formFieldValidationInfo.FormFieldValidationRules, new JsonSerializerOptions() { Converters = { new ValidationRuleConfigurationConverter(validationRuleDefinitionProvider) } });
            return validationRules;
        }
    }

}