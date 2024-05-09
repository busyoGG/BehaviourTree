using System;
using System.Runtime.CompilerServices;

/// <summary>
/// 按钮 参数name：按钮名称
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class E_Button : EBase
{
    private readonly string _name;
    public E_Button(string name, [CallerLineNumber] int lineNumber = 0)
    {
        _name = name;
        lineNum = lineNumber;
    }

    public string GetName()
    {
        return _name;
    }
}
