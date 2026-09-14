using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;


namespace Multiplayer.Utils;
#if DEBUG
internal static class ExportSaveData
{
    public static void DumpSaveData(bool updateData = true)
    {
        var path = Path.Combine(Application.persistentDataPath, $"Save {DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json");

        Multiplayer.Log($"Exporting save data to {path}...");

        // Ensure save data is up to date
        if (updateData)
            SaveGameManager.Instance.UpdateInternalData();

        try
        {
            // Get save data and clone
            var jsonObject = SaveGameManager.Instance.data.GetJsonObject().DeepClone();

            // Cleanup nested JSON strings
            ProcessNestedJson(jsonObject);

            // Pretty print and write to file
            var prettyJson = jsonObject.ToString(Formatting.Indented);

            File.WriteAllText(path, prettyJson);

            Multiplayer.Log($"Export complete!");
        }
        catch (Exception ex)
        {
            Multiplayer.LogError($"Failed to export save data:\r\n{ex}");
        }
    }

    static void ProcessNestedJson(JToken token)
    {
        if (token is JObject obj)
        {
            foreach (var property in obj.Properties().ToList())
            {
                if (property.Value is JValue jsonValue && jsonValue.Type == JTokenType.String)
                {
                    // Try to parse string values as JSON
                    var strValue = jsonValue.Value<string>();
                    if (!string.IsNullOrWhiteSpace(strValue) && (strValue.TrimStart().StartsWith("{") || strValue.TrimStart().StartsWith("[")))
                    {
                        try
                        {
                            var parsedToken = JToken.Parse(strValue);
                            property.Value = parsedToken;
                            ProcessNestedJson(parsedToken);
                        }
                        catch
                        {
                            // If parsing fails, keep the original string value
                        }
                    }
                }
                else if (property.Value != null)
                {
                    ProcessNestedJson(property.Value);
                }
            }
        }
        else if (token is JArray array)
        {
            for (int i = 0; i < array.Count; i++)
            {
                if (array[i] != null)
                {
                    ProcessNestedJson(array[i]);
                }
            }
        }
    }
}
#endif
