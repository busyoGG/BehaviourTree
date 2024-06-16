using System.Collections.Generic;

namespace BhTree
{
    public class SaveJson
    {
        public List<SaveJson> children = new List<SaveJson>();

        public dynamic data;

        public string type;
    }
}