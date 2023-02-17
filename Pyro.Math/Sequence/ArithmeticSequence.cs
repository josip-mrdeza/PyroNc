using System;

namespace Pyro.Math.Sequence;

public readonly struct ArithmeticSequence : ISequence
{
    public ArithmeticSequence(float firstElement, float difference)
    {
        FirstElement = firstElement;
        Difference = difference;
    }
    public float FirstElement { get; }
    public float Difference { get; }

    public float Sum(int n)
    {
        var sum = (n * 0.5f) * (FirstElement + GetElement(n));

        return sum;
    }

    public float GetElement(int n)
    {
        if (n < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "General element of an arithmetic sequence must be greater than or equal to 1 (n >= 1).");
        }
        var an = FirstElement + (n - 1) * Difference;

        return an;
    }

    public float this[int n] => GetElement(n);
}