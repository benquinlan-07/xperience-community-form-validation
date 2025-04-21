using System.Collections.Generic;
using Kentico.Forms.Web.Mvc;
using Kentico.Forms.Web.Mvc.Widgets;
using System.Linq;
using System.Text.Json;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using XperienceCommunity.FormValidation.Components.ValidationRuleList;
using XperienceCommunity.FormValidation.JsonConverters;
using XperienceCommunity.FormValidation.Models;

namespace XperienceCommunity.FormValidation
{
    public class FormComponentEvents
    {
        internal static void GetFormConfiguration(object sender, GetFormWidgetRenderingConfigurationEventArgs e)
        {
        }

        internal static void GetFormFieldConfiguration(object sender, GetFormFieldRenderingConfigurationEventArgs e)
        {
            AddClientsideValidationAttributes(e);
        }

        private static void AddClientsideValidationAttributes(GetFormFieldRenderingConfigurationEventArgs e)
        {
            var formFieldValidationInfoProvider = Service.Resolve<IInfoProvider<FormFieldValidationInfo>>();
            var formFieldValidation = formFieldValidationInfoProvider.Get()
                .WhereEquals(nameof(FormFieldValidationInfo.FormFieldValidationFieldGUID), e.FormComponent.BaseProperties.Guid)
                .FirstOrDefault();

            // Add base validator
            e.Configuration.EditorHtmlAttributes["data-val"] = "true";

            if (e.FormComponent.BaseProperties.Required)
            {
                if (!string.IsNullOrWhiteSpace(formFieldValidation?.FormFieldValidationRequiredError))
                    e.Configuration.EditorHtmlAttributes["data-val-required"] = formFieldValidation.FormFieldValidationRequiredError;
                else
                    e.Configuration.EditorHtmlAttributes["data-val-required"] = ResHelper.GetString("general.requiresvalue");
            }

            // Add email validator
            if (e.FormComponent is EmailInputComponent)
            {
                e.Configuration.EditorHtmlAttributes["data-val-email"] = "Please enter a valid email address.";
            }

            // Add number validator
            if (e.FormComponent is IntInputComponent)
            {
                e.Configuration.EditorHtmlAttributes["data-val-number"] = "Please enter a valid number.";
            }

            if (!string.IsNullOrWhiteSpace(formFieldValidation?.FormFieldValidationRules))
            {
                var validationRuleDefinitionProvider = Service.Resolve<IValidationRuleDefinitionProvider>();
                var validationRules = JsonSerializer.Deserialize<CustomValidationRuleConfiguration[]>(formFieldValidation.FormFieldValidationRules, new JsonSerializerOptions() { Converters = { new ValidationRuleConfigurationConverter(validationRuleDefinitionProvider) } });
                foreach (var config in validationRules)
                {
                    ApplyValidationRuleAttribute(config.RuleValues, e.Configuration.EditorHtmlAttributes);
                }
            }
            else
            {
                foreach (var rule in e.FormComponent.BaseProperties.ValidationRuleConfigurations)
                {
                    ApplyValidationRuleAttribute(rule.ValidationRule, e.Configuration.EditorHtmlAttributes);
                }
            }
        }

        private static void ApplyValidationRuleAttribute(ValidationRule validationRule, IDictionary<string, object> htmlAttributes)
        {
            if (validationRule is RegularExpressionValidationRule regexRule)
            {
                htmlAttributes["data-val-regex-pattern"] = regexRule.RegularExpression;
                htmlAttributes["data-val-regex"] = regexRule.ErrorMessage;
            }
            else if (validationRule is MinimumLengthValidationRule minLengthRule)
            {
                htmlAttributes["data-val-length-min"] = minLengthRule.MinimumLength;
                htmlAttributes["data-val-length"] = minLengthRule.ErrorMessage;
            }
            else if (validationRule is MaximumLengthValidationRule maxLengthRule)
            {
                htmlAttributes["data-val-length-max"] = maxLengthRule.MaximumLength;
                htmlAttributes["data-val-length"] = maxLengthRule.ErrorMessage;
            }
            else if (validationRule is MinimumIntValueValidationRule minIntRule)
            {
                htmlAttributes["data-val-range-min"] = minIntRule.MinimumValue;
                htmlAttributes["data-val-range"] = minIntRule.ErrorMessage;
            }
            else if (validationRule is MaximumIntValueValidationRule maxIntRule)
            {
                htmlAttributes["data-val-range-max"] = maxIntRule.MaximumValue;
                htmlAttributes["data-val-range"] = maxIntRule.ErrorMessage;
            }
            else if (validationRule is BoolIsSetValidationRule boolSetRule)
            {
                htmlAttributes["data-val-istrue"] = boolSetRule.ErrorMessage;
                htmlAttributes["class"] += " is-required-checked";
            }
        }
    }
}