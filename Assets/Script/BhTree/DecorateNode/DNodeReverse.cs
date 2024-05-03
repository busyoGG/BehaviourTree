namespace BhTree
{
    public class DNodeReverse: BhBaseNode
    {
        protected override BhResult GetResult()
        {
            BhResult res = base.GetResult();
            switch (res)
            {
                case BhResult.Success:
                    return BhResult.Fail;
                case BhResult.Fail:
                    return BhResult.Success;
                default:
                    return BhResult.Running;
            }
        }
    }
}