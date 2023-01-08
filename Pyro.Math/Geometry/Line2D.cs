using System;
using System.Globalization;

namespace Pyro.Math.Geometry;

public struct Line2D
{
    public readonly Vector2D Start;
    public readonly Vector2D End;
    public readonly decimal K = 0;
    public readonly string ExplicitEquation;
    public readonly string ImplictEquation;

    public Line2D(Vector2D p1, Vector2D p2)
    {
        if (float.IsNaN(p1.x) || float.IsNaN(p1.y))
        {
            throw new Exception($"Vector2D p1 in constructor Line2D had a NaN value {p2.ToString()}.");
        }

        if (float.IsNaN(p2.x) || float.IsNaN(p2.y))
        {
            throw new Exception($"Vector2D p2 in constructor Line2D had a NaN value {p2.ToString()}.");
        }
        
        Start = p1;
        End = p2;
        
        var x1 = p1.x;
        var y1 = p1.y;

        var x2 = p2.x;
        var y2 = p2.y;

        var f1 = (decimal) (y2 - y1);
        var f2 = (decimal) (x2 - x1);
        if (f2 == 0)
        {
            K = 0;
        }
        else
        {
            K = f1 / f2;
        }

        char sign = K > 0 ? '-' : '+';
            
        ExplicitEquation = $"y = {decimal.Round(K).ToString(CultureInfo.InvariantCulture)}x {sign.ToString(CultureInfo.InvariantCulture)}{System.Math.Abs(decimal.Round(K * (decimal) x1) - (decimal) y1).ToString(CultureInfo.InvariantCulture)}";
        ImplictEquation = null;
    }

    public float GetY(decimal x)
    {
        return (float) (K * x - (K * (decimal) Start.x - (decimal) Start.y));
    }
    
    
    public LineEquationResults FindCircleEndPoint(float radius, float p, float q)
    {
        //(x-p)^2 + (y-q)^2 = radius^2
        //(x-p)^2 + (Kx-C-q)^2 = radius^2
        //(x^2 - 2(x*p) + p^2) + ((Kx)^2 - 2(Kx*(C-q)) + (C-q)^2 - radius^2 = 0
        var xSq = 0m;
        var x = 0m;
        var num = 0m;
        var c = -(K * (decimal) Start.x - (decimal) Start.y);
        
        
        xSq += 1;
        x -= 2m * (decimal) p;
        num +=  ((decimal) p).Squared() - ((decimal) radius).Squared();

        xSq += K.Squared();
        x += 2m * K * (c - (decimal) q);
        num += (c - (decimal) q).Squared();

        var a = xSq;
        var b = x;
        var cc = num;

        var discriminant = (b.Squared() - (4 * a * cc));
        LineEquationResults.DiscriminantResult result;
        if (discriminant < 0)
        {
            result = LineEquationResults.DiscriminantResult.Imaginary;
            discriminant = 0;
        }
        else if (discriminant == 0)
        {
            result = LineEquationResults.DiscriminantResult.One;
        }
        else
        {
            result = LineEquationResults.DiscriminantResult.Dual;
        }
        
        var rootOfDiscriminant = discriminant.SquareRoot();
        var pos = (-b + rootOfDiscriminant) / (2 * a);
        var pos2 = (-b - rootOfDiscriminant) / (2 * a);

        return new LineEquationResults(new Vector2D((float) pos, GetY(pos)), new Vector2D((float) pos2, GetY(pos2)), result);
    }

    public struct LineEquationResults
    {
        public Vector2D Result1;
        public Vector2D Result2;
        public readonly DiscriminantResult ResultOfDiscriminant;

        public LineEquationResults(Vector2D result1, Vector2D result2, DiscriminantResult resultOfDiscriminant)
        {
            Result1 = result1;
            Result2 = result2;
            ResultOfDiscriminant = resultOfDiscriminant;
        }
        public enum DiscriminantResult
        {
            Dual,
            One,
            Imaginary
        }
    }
    
}