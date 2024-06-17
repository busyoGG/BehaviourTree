namespace BhTree
{
    public class CNodeSequence: BhBaseNode
    {
        public override bool CheckState(BhResult res)
        {
            if (res == BhResult.Fail || res == BhResult.Running)
            {
                return false;
            }
            
            return true;
        }
        
        public override bool CheckStop()
        {
            if (currentChildIndex >= children.Count || result == BhResult.Fail)
            {
                return true;
            }

            return false;
        }
    }
}