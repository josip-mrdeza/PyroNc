using System;

namespace Pyro.Math.Sequence;

public struct GeometricalSequence : ISequence
{
    public GeometricalSequence(float firstElement, float difference)
    {
        FirstElement = firstElement;
        Difference = difference;
    }
    public float FirstElement { get; }
    public float Difference { get; }

    public float Sum(int n)
    {
        var sum = FirstElement * ((1 - Difference.Pow(n)) / 1 - Difference);

        return sum;
    }

    public float GetElement(int n)
    {
        var nthElement = FirstElement * Difference.Pow(n - 1);

        return nthElement;
    }

    public float this[int n] => GetElement(n);
}