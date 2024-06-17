namespace BhTree
{
    public class DNodeSuccess:BhBaseNode
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
