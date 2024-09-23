using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class GetGlbCategory
{
    public List<string> GetCategories(string json)
    {
        JObject jsonObject = JObject.Parse(json);

        // Find the schema with "name": "category"
        JToken categorySchema = jsonObject["schema"]?
            .FirstOrDefault(schema => schema["name"]?.ToString() == "category");

        // Get the "options" > "values" array
        JArray values = categorySchema?["options"]?["values"] as JArray;

        if (values != null)
        {
            var list = new List<string>();
            Debug.Log("Category values:");
            foreach (var value in values)
            {
                Debug.Log(value);
                list.Add(value.ToString());
            }

            return list;
        }
        else
        {
            Debug.Log("Values not found.");
            return null;
        }
    }
}