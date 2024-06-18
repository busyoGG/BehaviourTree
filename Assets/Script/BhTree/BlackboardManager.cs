using System.Collections.Generic;

namespace BhTree
{
    public class BlackboardManager<T>: Singleton<BlackboardManager<T>>
    {
        private Dictionary<string, T> _blackboard = new Dictionary<string, T>();
        
        public void SetBlackboard(string key, T data)
        {
            _blackboard[key] = data;
        }

        public T GetBlackboard(string key)
        {
            _blackboard.TryGetValue(key, out var data);
            return data;
        }
    }
}