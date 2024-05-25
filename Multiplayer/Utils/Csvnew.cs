using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Multiplayer.Utils
{
    public static class Csv
    {
        public static ReadOnlyDictionary<string, Dictionary<string, string>> Parse(string data)
        {
            var columns = new Dictionary<string, Dictionary<string, string>>();
            var lines = data.Split('\n');

            var keys = ParseLine(lines[0]);
            foreach (var key in keys)
                columns[key] = new Dictionary<string, string>();

            for (int i = 0; i < lines.Length; i++)
            {
                var values = ParseLine(lines[i]);
                if (values.Count == 0 || string.IsNullOrWhiteSpace(values[0]))
                    continue;

                string key = values[0];
                for (int j = 0; j < values.Count; j++)
                    columns[keys[j]][key] = values[j];
            }

            return new ReadOnlyDictionary<string, Dictionary<string, string>>(columns);
        }

        private static List<string> ParseLine(string line)
        {
            var values = new List<string>();
            var builder = new StringBuilder();

            bool inQuotes = false;
            foreach (char c in line)
            {
                if (c == ',' && !inQuotes)
                {
                    values.Add(builder.ToString());
                    builder.Clear();
                }
                else if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else
                {
                    builder.Append(c);
                }
            }

            values.Add(builder.ToString());
            return values;
        }

        public static string Dump(ReadOnlyDictionary<string, Dictionary<string, string>> data)
        {
            var result = new StringBuilder();

            foreach (var column in data)
                result.Append($"{column.Key},");

            result.Length--; 
            result.Append('\n');

            int rowCount = data.Values.FirstOrDefault()?.Count ?? 0;

            for (int i = 0; i < rowCount; i++)
            {
                foreach (var column in data)
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

                result.Length--;
                result.Append('\n');
            }

            return result.ToString();
        }
    }
}
