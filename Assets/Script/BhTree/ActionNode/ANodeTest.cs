namespace BhTree
{
    public class ANodeTest : BhBaseNode
    {
        private bool _res;

        public override void Init(dynamic data)
        {
            _res = data.isSuc;
        }

        public override void Run()
        {
            if (_res)
            {
                result = BhResult.Success;
            }
            else
            {
                result = BhResult.Fail;
            }
        }
    }
}