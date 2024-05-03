namespace BhTree
{
    public class CNodeSelect: BhBaseNode
    {
        protected override BhResult GetResult()
        {
            BhResult res = BhResult.Fail;
            foreach (var node in _children)
            {
                res = node.Run();
                if (res != BhResult.Fail)
                {
                    return res;
                }
            }

            return res;
        }
    }
}