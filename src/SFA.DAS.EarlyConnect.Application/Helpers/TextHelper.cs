using System;
using System.Collections.Generic;

namespace SFA.DAS.EarlyConnect.Application.Helpers
{
    public static class TextHelper
    {
        public static string ExtractText(string input)
        {
            return input.Trim('!', ' ');
        }

        public static decimal ParseDecimal(IDictionary<string, object> dict, string key) =>
            decimal.TryParse(dict.TryGetValue(key, out var value) ? value?.ToString()?.Trim() : "0", out var result) ? result : 0;

        public static int ParseInteger(IDictionary<string, object> dict, string key) =>
            int.TryParse(dict.TryGetValue(key, out var value) ? value?.ToString()?.Trim() : "0", out var result) ? result : 0;

        public static bool ParseBoolean(IDictionary<string, object> dict, string key)
        {
            if (dict.TryGetValue(key, out var value))
            {
                var trimmedValue = value?.ToString()?.Trim()?.ToLowerInvariant();
                return IsSpecifiedValue(trimmedValue);
            }

            return false;
        }

        public static Guid ExtractGuid(string input)
        {
            input = ExtractText(input);
            return Guid.TryParse(input, out var output) ? output : Guid.Empty;
        }


        public static bool IsSpecifiedValue(string value) => value == "1" || value == "Y" || value == "YES";
    }
}
