using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using CMS.Core;
using Kentico.Forms.Web.Mvc;
using XperienceCommunity.FormValidation.Components.ValidationRuleList;
using XperienceCommunity.FormValidation.Extensions;

namespace XperienceCommunity.FormValidation.JsonConverters;

/// <summary>
/// Converts <see cref="T:Kentico.Xperience.Admin.Base.Forms.ValidationRuleConfiguration" /> to JSON format.
/// </summary>
internal sealed class ValidationRuleConfigurationConverter :
	JsonConverter<CustomValidationRuleConfiguration>
{
    private readonly IValidationRuleDefinitionProvider _validationRuleDefinitionProvider;

    private readonly string IdentifierPropertyName = ValidationRuleConfigurationConverter.GetPropertyName("Identifier");
    private readonly string ValidationRuleIdentifierPropertyName = ValidationRuleConfigurationConverter.GetPropertyName("ValidationRuleIdentifier");
    private readonly string RuleValuesPropertyName = ValidationRuleConfigurationConverter.GetPropertyName("RuleValues");

    public ValidationRuleConfigurationConverter()
	    : this(Service.Resolve<IValidationRuleDefinitionProvider>())
    {
    }

    public ValidationRuleConfigurationConverter(IValidationRuleDefinitionProvider validationRuleDefinitionProvider)
    {
        _validationRuleDefinitionProvider = validationRuleDefinitionProvider;
    }

    /// <inheritdoc />
	public override bool CanConvert(Type typeToConvert)
	{
		return typeToConvert.IsAssignableFrom(typeof(CustomValidationRuleConfiguration));
	}

	/// <inheritdoc />
	public override CustomValidationRuleConfiguration Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException();
		CustomValidationRuleConfiguration ruleConfiguration = new CustomValidationRuleConfiguration();
		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.EndObject)
				return ruleConfiguration;
			if (reader.TokenType != JsonTokenType.PropertyName)
				throw new JsonException();
			string str = reader.GetString();
			reader.Read();
            if (str.Equals(this.IdentifierPropertyName, StringComparison.Ordinal))
                ruleConfiguration.Identifier = reader.GetString();
            if (str.Equals(this.ValidationRuleIdentifierPropertyName, StringComparison.Ordinal))
                ruleConfiguration.ValidationRuleIdentifier = reader.GetString();
            else if (str.Equals(this.RuleValuesPropertyName, StringComparison.Ordinal))
            {
                var propertiesType = _validationRuleDefinitionProvider.Get(ruleConfiguration.ValidationRuleIdentifier)?.ValidationRuleType;
				if (propertiesType != null)
                {
                    JsonSerializerOptions options1 = new JsonSerializerOptions(options)
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    ruleConfiguration.RuleValues = (Kentico.Forms.Web.Mvc.ValidationRule)JsonSerializer.Deserialize(ref reader, propertiesType, options1);
                }
            }
        }
		throw new JsonException();
	}

	/// <inheritdoc />
	public override void Write(
		Utf8JsonWriter writer,
		CustomValidationRuleConfiguration value,
		JsonSerializerOptions options)
	{
		writer.WriteStartObject();
        if (value.Identifier != null)
        {
            writer.WritePropertyName(this.IdentifierPropertyName);
            JsonSerializer.Serialize<string>(writer, value.Identifier, options);
        }
        if (value.ValidationRuleIdentifier != null)
		{
			writer.WritePropertyName(this.ValidationRuleIdentifierPropertyName);
			JsonSerializer.Serialize<string>(writer, value.ValidationRuleIdentifier, options);
		}
		if (value.RuleValues != null)
		{
			writer.WritePropertyName(this.RuleValuesPropertyName);
			JsonSerializer.Serialize(writer, value.RuleValues, value.RuleValues.GetType(), options);
		}
		writer.WriteEndObject();
	}

	private static string GetPropertyName(string propertyName)
    {
        return propertyName.ToCamelCase();
	}
}