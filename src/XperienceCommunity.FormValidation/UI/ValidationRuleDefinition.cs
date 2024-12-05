using System;
using System.Runtime.CompilerServices;
using Kentico.Xperience.Admin.Base.Forms;

namespace XperienceCommunity.FormValidation.UI;

/// <summary>Definition of a registered validation rule.</summary>
internal class ValidationRuleDefinition
{
	/// <summary>Type implementing the validation rule.</summary>
	public Type RuleType { get; }

	/// <summary>Unique identifier of the validation rule.</summary>
	public string Identifier { get; }

	/// <summary>Name of the validation rule.</summary>
	public string Name { get; }

	/// <summary>Description of the validation rule.</summary>
	public string Description { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Kentico.Xperience.Admin.Base.Forms.ValidationRuleDefinition" /> class.
	/// </summary>
	/// <param name="identifier">Unique identifier of the validation rule.</param>
	/// <param name="ruleType">Type implementing the validation rule.</param>
	/// <param name="name">Name of the validation rule.</param>
	/// <param name="description">Description of the validation rule.</param>
	public ValidationRuleDefinition(
		string identifier,
		Type ruleType,
		string name,
		string description = null)
	{
		//DefinitionValidationUtils.ValidateIdentifier(identifier);
		//DefinitionValidationUtils.ValidateImplementingTypeWithGenericBase(ruleType, typeof(ValidationRule<,,>));
		//DefinitionValidationUtils.ValidateName(name);
		Identifier = identifier;
		Name = name;
		RuleType = ruleType;
		Description = description;
	}

	/// <summary>
	/// Gets the type of the generic parameter <c>TProperties</c> in the <see cref="T:Kentico.Xperience.Admin.Base.Forms.ValidationRule`3" />
	/// that definitions <see cref="P:Kentico.Xperience.Admin.Base.Forms.ValidationRuleDefinition.RuleType" /> inherits.
	/// </summary>
	public Type GetPropertiesType()
	{
		Type genericTypeArgument = GetValidationRuleBaseType().GenericTypeArguments[0];
		if (genericTypeArgument.IsAssignableTo(typeof(ValidationRuleProperties)))
			return genericTypeArgument;
		DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(47, 2);
		interpolatedStringHandler.AppendLiteral("Registered validation rule ");
		interpolatedStringHandler.AppendFormatted<Type>(RuleType);
		interpolatedStringHandler.AppendLiteral(" must inherit ");
		interpolatedStringHandler.AppendFormatted(typeof(ValidationRule<,,>).FullName);
		interpolatedStringHandler.AppendLiteral(" type.");
		throw new InvalidOperationException(interpolatedStringHandler.ToStringAndClear());
	}

	/// <summary>
	/// Gets the type of the generic parameter <c>TValue</c> in the <see cref="T:Kentico.Xperience.Admin.Base.Forms.ValidationRule`3" />
	/// that definitions <see cref="P:Kentico.Xperience.Admin.Base.Forms.ValidationRuleDefinition.RuleType" /> inherits.
	/// </summary>
	public Type GetValueType() => GetValidationRuleBaseType().GenericTypeArguments[2];

	private Type GetValidationRuleBaseType()
	{
		for (Type validationRuleBaseType = RuleType; validationRuleBaseType != null; validationRuleBaseType = validationRuleBaseType.BaseType)
		{
			if (validationRuleBaseType.IsGenericType && validationRuleBaseType.GetGenericTypeDefinition() == typeof(ValidationRule<,,>))
				return validationRuleBaseType;
		}
		DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(47, 2);
		interpolatedStringHandler.AppendLiteral("Registered validation rule ");
		interpolatedStringHandler.AppendFormatted<Type>(RuleType);
		interpolatedStringHandler.AppendLiteral(" must inherit ");
		interpolatedStringHandler.AppendFormatted(typeof(ValidationRule<,,>).FullName);
		interpolatedStringHandler.AppendLiteral(" type.");
		throw new InvalidOperationException(interpolatedStringHandler.ToStringAndClear());
	}
}