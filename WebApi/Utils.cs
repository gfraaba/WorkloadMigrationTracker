using System;
using System.Text.RegularExpressions;

namespace WebApi.Utils
{
    public static class Utils
    {
        /// <summary>
        /// Replaces environment variables in the connection string with their values.
        /// </summary>
        /// <param name="connectionString">The connection string containing environment variables.</param>
        /// <returns>The connection string with environment variables replaced by their values.</returns>
        /// <remarks>
        /// This method uses a regular expression to find patterns of the form ${VAR_NAME}
        /// and replaces them with the corresponding environment variable value.
        /// </remarks>
        /// <example>
        /// var connectionString = "Server=${DB_SERVER};Database=${DB_NAME};User Id=${DB_USER};Password=${DB_PASSWORD};";
        /// var updatedConnectionString = Utils.ReplaceEnvironmentVariables(connectionString);
        /// </example>
        
        public static string ReplaceEnvironmentVariables(string connectionString)
        {
            // Find all ${VAR_NAME} patterns and replace with environment variable values
            return Regex.Replace(connectionString, @"\${([^}]+)}", match =>
            {
                string varName = match.Groups[1].Value;
                string? value = Environment.GetEnvironmentVariable(varName);
                Console.WriteLine($"Replacing ${{{varName}}} with {value ?? "null"}");
                return value ?? string.Empty;
            });
        }
    }
}