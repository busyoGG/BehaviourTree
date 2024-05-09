using System;
using System.Runtime.CompilerServices;

/// <summary>
/// 标签
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class E_Label : EBase
{
    /// <summary>
    /// 标签
    /// </summary>
    /// <param name="lineNumber"></param>
    public E_Label([CallerLineNumber] int lineNumber = 0) {
        lineNum = lineNumber;
    }
}
