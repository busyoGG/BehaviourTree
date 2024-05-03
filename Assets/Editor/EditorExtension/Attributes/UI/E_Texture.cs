using System;
using System.Runtime.CompilerServices;

/// <summary>
/// ???
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class E_Texture : EBase
{
    public E_Texture([CallerLineNumber] int lineNumber = 0)
    {
        lineNum = lineNumber;
    }
}
