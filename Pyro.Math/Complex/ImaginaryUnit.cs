using System;
using System.Globalization;

namespace Pyro.Math.Complex;

public readonly struct ImaginaryUnit 
{
    public readonly float Exponent;
    public ImaginaryUnit Reduced => new ImaginaryUnit(Exponent % 4);
    public float Number => ResolveExponent();
    private readonly string _cachedString;
    public ImaginaryUnit(float exponent)
    {
        Exponent = exponent;
        _cachedString = $"i^{exponent.ToString(CultureInfo.InvariantCulture)}";
    }

    public float ResolveExponent()
    {
        ImaginaryUnit unit = this;
        if (unit.Exponent > 4)
        {
            unit = unit.Reduced;
        }

        return unit.Exponent switch
        {
            0 => 1,
            1 => float.NaN, //sqroot
            2 => -1,
            3 => float.NaN, //sqroot,
            4 => -1,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public override string ToString()
    {
        return _cachedString;
    }
}