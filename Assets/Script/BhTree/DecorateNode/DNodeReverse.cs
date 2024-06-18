namespace BhTree
{
    public class DNodeReverse: BhBaseNode
    {
        public override void Run()
        {
            switch (result)
            {
                case BhResult.Fail:
                    SetResult(BhResult.Success);
                    break;
                case BhResult.Success:
                    SetResult(BhResult.Fail);
                    break;
            }
        }
    }
}