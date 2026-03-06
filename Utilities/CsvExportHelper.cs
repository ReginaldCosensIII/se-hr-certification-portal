using System;
using System.Collections.Generic;
using System.Text;

namespace SeHrCertificationPortal.Utilities
{
    public static class CsvExportHelper
    {
        public static string GenerateCsv<T>(IEnumerable<T> data, string[] headers, Func<T, string[]> rowSelector)
        {
            var csvBuilder = new StringBuilder();

            // Add headers
            csvBuilder.AppendLine(string.Join(",", EscapeRow(headers)));

            // Add rows
            foreach (var item in data)
            {
                var rowData = rowSelector(item);
                csvBuilder.AppendLine(string.Join(",", EscapeRow(rowData)));
            }

            return csvBuilder.ToString();
        }

        private static IEnumerable<string> EscapeRow(string[] row)
        {
            foreach (var field in row)
            {
                if (string.IsNullOrEmpty(field))
                {
                    yield return "\"\"";
                }
                else
                {
                    yield return $"\"{field.Replace("\"", "\"\"")}\"";
                }
            }
        }
    }
}
