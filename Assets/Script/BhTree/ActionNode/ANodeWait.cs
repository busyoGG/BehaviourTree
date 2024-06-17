using UnityEngine;

namespace BhTree
{
    public class ANodeWait : BhBaseNode
    {
        private int _time;

        private int _curTime = 0;

        private int _defTime = -1;

        public override void Init(dynamic data)
        {
            _time = data.time;
        }

        public override void Run()
        {
            if (_defTime == -1)
            {
                _defTime = (int)Time.time * 1000;
            }

            _curTime = (int)Time.time * 1000 - _defTime;
            
            if (_curTime > _time)
            {
                result = BhResult.Success;
            }
            else
            {
                result = BhResult.Running;
            }
        }

        public override void Reset()
        {
            base.Reset();
            _curTime = 0;
        }
    }
}