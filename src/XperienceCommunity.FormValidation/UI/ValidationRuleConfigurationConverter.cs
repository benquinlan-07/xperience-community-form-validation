using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace XperienceCommunity.FormValidation.UI;

/// <summary>
/// Converts <see cref="T:Kentico.Xperience.Admin.Base.Forms.ValidationRuleConfiguration" /> to JSON format.
/// </summary>
internal sealed class ValidationRuleConfigurationConverter :
	JsonConverter<Kentico.Forms.Web.Mvc.ValidationRuleConfiguration>
{
	private readonly string IdentifierPropertyName = ValidationRuleConfigurationConverter.GetPropertyName("Identifier");
	private readonly string ValidationRuleIdentifierPropertyName = ValidationRuleConfigurationConverter.GetPropertyName("ValidationRuleIdentifier");
	private readonly string RuleValuesPropertyName = ValidationRuleConfigurationConverter.GetPropertyName("RuleValues");

	/// <inheritdoc />
	public override bool CanConvert(Type typeToConvert)
	{
		return typeToConvert.IsAssignableFrom(typeof(Kentico.Forms.Web.Mvc.ValidationRuleConfiguration));
	}

	/// <inheritdoc />
	public override Kentico.Forms.Web.Mvc.ValidationRuleConfiguration Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException();
		Kentico.Forms.Web.Mvc.ValidationRuleConfiguration ruleConfiguration = new Kentico.Forms.Web.Mvc.ValidationRuleConfiguration();
		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.EndObject)
				return ruleConfiguration;
			if (reader.TokenType != JsonTokenType.PropertyName)
				throw new JsonException();
			string str = reader.GetString();
			reader.Read();
			if (str.Equals(this.ValidationRuleIdentifierPropertyName, StringComparison.Ordinal))
				ruleConfiguration.Identifier = reader.GetString();
			else if (str.Equals(this.RuleValuesPropertyName, StringComparison.Ordinal))
			{
				Type propertiesType = typeof(string);// DefinitionStore<ValidationRuleDefinitionStore, ValidationRuleDefinition>.Instance.Get(ruleConfiguration.ValidationRuleIdentifier).GetPropertiesType();
				JsonSerializerOptions options1 = new JsonSerializerOptions(options)
				{
					PropertyNameCaseInsensitive = true
				};

				ruleConfiguration.ValidationRule = (Kentico.Forms.Web.Mvc.ValidationRule)JsonSerializer.Deserialize(ref reader, propertiesType, options1);
			}
		}
		throw new JsonException();
	}

	/// <inheritdoc />
	public override void Write(
		Utf8JsonWriter writer,
		Kentico.Forms.Web.Mvc.ValidationRuleConfiguration value,
		JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		if (value.Identifier != null)
		{
			writer.WritePropertyName(this.IdentifierPropertyName);
			JsonSerializer.Serialize<string>(writer, value.Identifier, options);
		}
		if (value.Identifier != null)
		{
			writer.WritePropertyName(this.ValidationRuleIdentifierPropertyName);
			JsonSerializer.Serialize<string>(writer, value.Identifier, options);
		}
		if (value.ValidationRule != null)
		{
			writer.WritePropertyName(this.RuleValuesPropertyName);
			JsonSerializer.Serialize(writer, value.ValidationRule, options);
		}
		writer.WriteEndObject();
	}

	private static string GetPropertyName(string propertyName)
	{
		return char.ToLower(propertyName[0]).ToString() + propertyName.Substring(1);
	}
}