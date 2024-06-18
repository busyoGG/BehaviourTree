namespace BhTree
{
    public class CNodeParallel: BhBaseNode
    {
        public override bool CheckState(BhResult res)
        {
            if (res == BhResult.Running)
            {
                return false;
            }
            return true;
        }

        public override bool CheckStop()
        {
            if (currentChildIndex >= children.Count)
            {
                return true;
            }

            return false;
        }
    }
}