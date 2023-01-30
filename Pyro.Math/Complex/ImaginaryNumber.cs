using System.Globalization;

namespace Pyro.Math.Complex;

public struct ImaginaryNumber
{
    private float _real;
    private float _imaginary;
    private string _cachedString;
    private bool _isDirty;

    public float Real
    {
        get => _real;
        set
        {
            _real = value;
            _isDirty = true;
        }
    }

    public float Imaginary
    {
        get => _imaginary;
        set
        {
            _imaginary = value;
            _isDirty = true;
        }
    }

    public float Modulus => (_real.Squared() + _imaginary.Squared()).SquareRoot();

    public ImaginaryNumber Conjugate => new ImaginaryNumber(_real, -_imaginary);

    public ImaginaryNumber(float real, float imaginary)
    {
        _real = real;
        _imaginary = imaginary;
        _isDirty = true;
        _cachedString = null;
    }

    public override string ToString()
    {
        if (_isDirty || _cachedString == null)
        {
            _cachedString = $"z ={(_real == 0 ? " " : "")}{_real.ToString(CultureInfo.InvariantCulture)} {_imaginary.ToString(CultureInfo.InvariantCulture)}i";
            _isDirty = false;
        }

        return _cachedString;
    }
}