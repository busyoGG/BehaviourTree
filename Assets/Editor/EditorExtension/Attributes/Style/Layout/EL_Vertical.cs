

using System;
/// <summary>
/// 垂直布局 参数isStart：true代表Start，false代表End
/// </summary>
public class EL_Vertical : Attribute
{
    private readonly bool _isStart;
    public EL_Vertical(bool isStart)
    {
        _isStart = isStart;
    }

    public bool IsStart()
    {
        return _isStart;
    }
}
