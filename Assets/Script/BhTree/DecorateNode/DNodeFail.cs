namespace BhTree
{
    public class DNodeFail : BhBaseNode
    {
        public override bool CheckState(BhResult res)
        {
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