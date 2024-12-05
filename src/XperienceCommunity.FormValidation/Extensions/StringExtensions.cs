using System.Text.RegularExpressions;

namespace XperienceCommunity.FormValidation.Extensions;

internal static class StringExtensions
{
	public static string ToCamelCase(this string input)
	{
		string input1 = input.Replace("_", string.Empty);
		if (input1.Length == 0)
			return input1;
		string str = Regex.Replace(input1, "([A-Z])([A-Z]+)($|[A-Z])", (MatchEvaluator)(m => m.Groups[1].Value + m.Groups[2].Value.ToLower() + m.Groups[3].Value));
		return char.ToLower(str[0]).ToString() + str.Substring(1);
	}
}