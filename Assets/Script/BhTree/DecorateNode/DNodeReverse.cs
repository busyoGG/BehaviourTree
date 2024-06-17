namespace BhTree
{
    public class DNodeReverse: BhBaseNode
    {
        public override void SetResult(BhResult result)
        {
            switch (result)
            {
                case BhResult.Fail:
                    base.SetResult(BhResult.Success);
                    break;
                case BhResult.Success:
                    base.SetResult(BhResult.Fail);
                    break;
            }
        }

        public override bool CheckState(BhResult res)
        {
            return true;
        }
        
        public override bool CheckStop()
        {
            return true;
        }
    }
}