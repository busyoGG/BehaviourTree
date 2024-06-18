using System.Collections.Generic;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Device;

namespace BhTree
{
    public class ConfigLoader
    {
        public static SaveJson Load(string path)
        {
            string jsonData = FileUtils.ReadFile(path);

            SaveJson json = JsonConvert.DeserializeObject<List<SaveJson>>(jsonData)[0];

            Deserialize(json);

            return json;
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="parent"></param>
        private static void Deserialize(SaveJson parent)
        {
            dynamic obj = new ExpandoObject();
            foreach (var res in (parent.data as JObject).Properties())
            {
                JTokenType jType = res.Value.Type;

                // ConsoleUtils.Log(dataObj);
                switch (jType)
                {
                    case JTokenType.String:
                        ((IDictionary<string, object>)obj)[res.Name] = res.Value.Value<string>();
                        break;
                    case JTokenType.Float:
                        ((IDictionary<string, object>)obj)[res.Name] = res.Value.Value<float>();
                        break;
                    case JTokenType.Integer:
                        ((IDictionary<string, object>)obj)[res.Name] = res.Value.Value<int>();
                        break;
                    case JTokenType.Boolean:
                        ((IDictionary<string, object>)obj)[res.Name] = res.Value.Value<bool>();
                        break;
                }
            }

            parent.data = obj;

            foreach (var child in parent.children)
            {
                Deserialize(child);
            }
        }
    }
}