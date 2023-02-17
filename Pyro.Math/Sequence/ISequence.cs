using System.Collections;
using System.Collections.Generic;

namespace Pyro.Math.Sequence;

public interface ISequence
{
    public float FirstElement { get; }
    public float Difference { get; }
    public float Sum(int n);
    public float GetElement(int n);
    public float this[int n]
    {
        get;
    }
}