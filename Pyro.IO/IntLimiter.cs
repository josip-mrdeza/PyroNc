namespace Pyro.IO;

public struct IntLimiter
{
    private int _max;
    private int _min;
    private int _val;
    public int Value
    {
        get
        {
            if (_val > _max)
            {
                _val = _max;
            }
            else if (_val < _min)
            {
                _val = _min;
            }

            return _val;
        }
        set => _val = value;
    }

    public IntLimiter(int value, int min = 0, int max = 9)
    {
        _min = min;
        _max = max;
        _val = value;
    }
    
    public static implicit operator int(IntLimiter limiter)
    {
        return limiter.Value;
    }

    public static implicit operator IntLimiter(int val)
    {
        return new IntLimiter(val);
    }

    public override string ToString() => Value.ToString();
}