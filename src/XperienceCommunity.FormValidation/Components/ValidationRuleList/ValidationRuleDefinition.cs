using System;
using System.Runtime.CompilerServices;
using Kentico.Xperience.Admin.Base.Forms;

namespace XperienceCommunity.FormValidation.Components.ValidationRuleList;

internal class ValidationRuleDefinition
{
	public Type RuleType { get; }
	public string Identifier { get; }
	public string Name { get; }
	public string Description { get; }
	
    public ValidationRuleDefinition(
		string identifier,
		Type ruleType,
		string name,
		string description = null)
	{
		Identifier = identifier;
		Name = name;
		RuleType = ruleType;
		Description = description;
	}

	public Type GetPropertiesType()
	{
		Type genericTypeArgument = GetValidationRuleBaseType().GenericTypeArguments[0];
		if (genericTypeArgument.IsAssignableTo(typeof(Kentico.Xperience.Admin.Base.Forms.ValidationRuleProperties)))
			return genericTypeArgument;
		DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(47, 2);
		interpolatedStringHandler.AppendLiteral("Registered validation rule ");
		interpolatedStringHandler.AppendFormatted<Type>(RuleType);
		interpolatedStringHandler.AppendLiteral(" must inherit ");
		interpolatedStringHandler.AppendFormatted(typeof(ValidationRule<,,>).FullName);
		interpolatedStringHandler.AppendLiteral(" type.");
		throw new InvalidOperationException(interpolatedStringHandler.ToStringAndClear());
	}

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