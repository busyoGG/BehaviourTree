using System.Collections.Generic;

namespace GraphViewExtension
{
    public class SaveJson
    {
        public List<SaveJson> children = new List<SaveJson>();

        public dynamic data;

        public string type;
    }
}