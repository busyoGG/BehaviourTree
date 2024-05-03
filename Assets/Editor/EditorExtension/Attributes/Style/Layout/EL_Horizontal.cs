

using System;

/// <summary>
/// 水平布局 参数isStart：true代表Start，false代表End
/// </summary>
public class EL_Horizontal : Attribute
{
    private readonly bool _isStart;
    public EL_Horizontal(bool isStart)
    {
        _isStart = isStart;
    }

    public bool IsStart()
    {
        return _isStart;
    }
}
