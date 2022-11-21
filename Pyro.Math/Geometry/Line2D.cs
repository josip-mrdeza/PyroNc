using System;
using System.Globalization;

namespace Pyro.Math.Geometry;

public struct Line2D
{
    public readonly Vector2D Start;
    public readonly Vector2D End;
    public readonly float K = 0;
    public readonly string ExplicitEquation;
    public readonly string ImplictEquation;

    public Line2D(Vector2D p1, Vector2D p2)
    {
        Start = p1;
        End = p2;
        
        var x1 = p1.x;
        var y1 = p1.y;

        var x2 = p2.x;
        var y2 = p2.y;

        var f1 = (y2 - y1);
        var f2 = (x2 - x1);

        K = f1 / f2;

        char sign = K > 0 ? '-' : '+';
            
        ExplicitEquation = $"y = {K.Round(2).ToString(CultureInfo.InvariantCulture)}x {sign.ToString(CultureInfo.InvariantCulture)}{System.Math.Abs((K * x1) - y1).Round(2).ToString(CultureInfo.InvariantCulture)}";
        ImplictEquation = null;
    }

    public float GetY(float x)
    {
        return (float) (((decimal) K * (decimal) x) - (((decimal) K * (decimal) Start.x) - (decimal) Start.y));
    }
    
    
    public (Vector2D positive, Vector2D negative) FindCircleEndPoint(float radius, float p, float q)
    {
        //(x-p)^2 + (y-q)^2 = radius^2
        //(x-p)^2 + (Kx-C-q)^2 = radius^2
        //(x^2 - 2(x*p) + p^2) + ((Kx)^2 - 2(Kx*(C-q)) + (C-q)^2 - radius^2 = 0
        var xSq = 0m;
        var x = 0m;
        var num = 0m;
        var c = -(((decimal) K * ((decimal) Start.x)) - (decimal) Start.y);
        
        
        xSq += 1;
        x -= 2m * (decimal) p;
        num +=  ((decimal) p).Squared() - ((decimal) radius).Squared();

        xSq += ((decimal) K).Squared();
        x += 2m * (decimal) K * (c - (decimal) q);
        num += (c - (decimal) q).Squared();

        var a = xSq;
        var b = x;
        var cc = num;

        var discriminant = (b.Squared() - (4 * a * cc)).SquareRoot();

        var pos = (-b + discriminant) / (2*a);
        var pos2 = (-b - discriminant) / (2 * a);

        return (new Vector2D((float) pos, GetY((float) pos)), new Vector2D((float) pos2, GetY((float) pos2)));
    }

    public Vector2D GetCorrectPoint((Vector2D, Vector2D) tuple, Vector2D pos)
    {
        var d = Space2D.Distance(tuple.Item1, pos);
        var d2 = Space2D.Distance(tuple.Item2, pos);

        return d > d2 ? tuple.Item2 : tuple.Item1;
    }
    
}