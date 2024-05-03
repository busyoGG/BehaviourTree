using System;
using System.Runtime.CompilerServices;

/// <summary>
/// ±Í«©
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class E_Label : EBase
{
    /// <summary>
    /// ±Í«©
    /// </summary>
    /// <param name="lineNumber"></param>
    public E_Label([CallerLineNumber] int lineNumber = 0) {
        lineNum = lineNumber;
    }
}
