using System.Text.RegularExpressions;

namespace PersonalWebApi.Utilities.Document
{
    public static class TextFormatter
    {
        /// <summary>
        /// Delete from beginning and end of the string all characters that are not letters or digits.
        /// If '''json["a", "b"]''' return ["a", "b"]
        /// If '''json["a", "b"]''' return {"a", "b"}
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static string CleanResponse(string input)
        {
            // Remove backticks and the "json" keyword
            string cleanedInput = input.Replace("'''", string.Empty).Replace("json", string.Empty).Trim();
            // Extract JSON object or array using regex
            Match match = Regex.Match(cleanedInput, @"(\{.*?\}|\[.*?\])");
            if (match.Success)
            {
                return match.Value;
            }
            return input;
        }
    }
}
