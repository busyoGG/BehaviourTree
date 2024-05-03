namespace BhTree
{
    public class CNodeParallel: BhBaseNode
    {
        protected override BhResult GetResult()
        {
            BhResult res = BhResult.Success;
            foreach (var node in _children)
            {
                res = node.Run();
                if (res != BhResult.Success)
                {
                    return res;
                }
            }
            
            return res;
        }
    }
}