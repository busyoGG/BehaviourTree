namespace BhTree
{
    public class DNodeFail : BhBaseNode
    {
        protected override BhResult GetResult()
        {
            return BhResult.Fail;
        }
    }

}