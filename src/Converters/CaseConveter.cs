using System.Globalization;
using System.Text;

namespace DevToys.JsonToPython.Converters;

internal static class CaseConveter
{
    public static string ToPascalCase(this string text)
    {
        var stringBuilder = new StringBuilder();
        var textInfo = CultureInfo.CurrentCulture.TextInfo;
        var flag = true;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (char.IsLetterOrDigit(c))
            {
                if (flag)
                {
                    stringBuilder.Append(textInfo.ToUpper(c));
                    flag = false;
                }
                else
                {
                    stringBuilder.Append((i < text.Length - 1 && char.IsUpper(c) && char.IsLower(text[i + 1])) ? c : char.ToLowerInvariant(c));
                }
            }
            else
            {
                flag = true;
            }

            if (i < text.Length - 1 && char.IsLower(text[i]) && char.IsUpper(text[i + 1]))
            {
                flag = true;
            }
        }

        return stringBuilder.ToString();
    }
}
