namespace BhTree
{
    public class CNodeSelect: BhBaseNode
    {
        public override void Init(dynamic data)
        {
            interruptCheck = data.interrupt;
        }

        public override bool CheckState(BhResult res)
        {
            if (res == BhResult.Success || res == BhResult.Running)
            {
                return false;
            }
            
            return true;
        }
        
        public override bool CheckStop()
        {
            if (currentChildIndex >= children.Count || result == BhResult.Success)
            {
                return true;
            }

            return false;
        }
    }
}