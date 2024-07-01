using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Multiplayer.Utils
{
    public static class Csv
    {
        /// <summary>
        /// Parses a CSV string into a dictionary of columns, each of which is a dictionary of rows, keyed by the first column.
        /// </summary>
        /// <param name="data">The CSV data as a string.</param>
        /// <returns>A read-only dictionary where each key is a column name and the value is a dictionary of rows.</returns>
        public static ReadOnlyDictionary<string, Dictionary<string, string>> Parse(string data)
        {
            // Split the input data into lines
            string[] lines = data.Split('\n');

            // Initialize an ordered dictionary to maintain the column order
            OrderedDictionary columns = new(lines.Length - 1);

            // Parse the first line to get the column headers
            List<string> keys = ParseLine(lines[0]);
            foreach (string key in keys)
                columns.Add(key, new Dictionary<string, string>());

            // Parse the remaining lines to fill in the column data
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                List<string> values = ParseLine(line);

                // Skip empty lines or lines with a blank first value
                if (values.Count == 0 || string.IsNullOrWhiteSpace(values[0]))
                    continue;

                string key = values[0];
                for (int j = 0; j < values.Count; j++)
                    ((Dictionary<string, string>)columns[j]).Add(key, values[j]);
            }

            // Convert the ordered dictionary to a read-only dictionary
            return new ReadOnlyDictionary<string, Dictionary<string, string>>(columns.Cast<DictionaryEntry>()
                .ToDictionary(entry => (string)entry.Key, entry => (Dictionary<string, string>)entry.Value));
        }

        /// <summary>
        /// Parses a single line of CSV data.
        /// </summary>
        /// <param name="line">The line to parse.</param>
        /// <returns>A list of values from the line.</returns>
        private static List<string> ParseLine(string line)
        {
            bool inQuotes = false;
            bool wasBackslash = false;
            List<string> values = new();
            StringBuilder builder = new();

            // Helper method to add the current value to the list and reset the builder
            void FinishLine()
            {
                values.Add(builder.ToString());
                builder.Clear();
            }

            // Iterate through each character in the line
            foreach (char c in line)
            {
                if (c == '\n' || (!inQuotes && c == ','))
                {
                    FinishLine();
                    continue;
                }

                switch (c)
                {
                    case '\r':
                        Multiplayer.LogWarning("Encountered carriage return in CSV! Please use Unix-style line endings (LF).");
                        continue;
                    case '"':
                        inQuotes = !inQuotes;
                        continue;
                    case '\\':
                        wasBackslash = true;
                        continue;
                }

                if (wasBackslash)
                {
                    wasBackslash = false;
                    if (c == 'n')
                    {
                        builder.Append('\n');
                        continue;
                    }

                    // Not a special character, so just append the backslash
                    builder.Append('\\');
                }

                builder.Append(c);
            }

            if (builder.Length > 0)
                FinishLine();

            return values;
        }

        /// <summary>
        /// Converts the dictionary data back to a CSV string.
        /// </summary>
        /// <param name="data">The dictionary data.</param>
        /// <returns>The CSV string representation of the data.</returns>
        public static string Dump(ReadOnlyDictionary<string, Dictionary<string, string>> data)
        {
            StringBuilder result = new("\n");

            // Write the column headers
            foreach (KeyValuePair<string, Dictionary<string, string>> column in data)
                result.Append($"{column.Key},");
            result.Remove(result.Length - 1, 1);
            result.Append('\n');

            int rowCount = data.Values.FirstOrDefault()?.Count ?? 0;

            // Write the rows
            for (int i = 0; i < rowCount; i++)
            {
                foreach (KeyValuePair<string, Dictionary<string, string>> column in data)
                {
                    if (column.Value.Count > i)
                    {
                        string value = column.Value.ElementAt(i).Value.Replace("\n", "\\n");
                        result.Append(value.Contains(',') ? $"\"{value}\"," : $"{value},");
                    }
                    else
                    {
                        result.Append(',');
                    }
                }
                result.Remove(result.Length - 1, 1);
                result.Append('\n');
            }

            return result.ToString();
        }
    }
}
