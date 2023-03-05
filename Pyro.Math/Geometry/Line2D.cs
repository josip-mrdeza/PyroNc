using System;
using System.Globalization;
using System.Security.Policy;

namespace Pyro.Math.Geometry;

public struct Line2D
{
    public readonly Vector2D Start;
    public readonly Vector2D End;
    public readonly float K = 0;
    public float Angle => GetAngle();
    public float AngleRadian => GetAngleRadian();
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

        var f1 = (float) (y2 - y1);
        var f2 = (float) (x2 - x1);
        if (f2 == 0)
        {
            K = 0;
        }
        else
        {
            K = f1 / f2;
        }

        char sign = K > 0 ? '-' : '+';
            
        ExplicitEquation = $"y = {K.Round().ToString(CultureInfo.InvariantCulture)}x {sign.ToString(CultureInfo.InvariantCulture)}{System.Math.Abs((K * (float) x1).Round() - (float) y1).ToString(CultureInfo.InvariantCulture)}";
        ImplictEquation = null;
    }

    public Line2D(float degree)
    {
        K = degree.Tan();
        if (degree > 180)
        {
            K = -K;
        }
        Start = new Vector2D();
        End = new Vector2D(degree.Cos(), degree.Sin());
        
        var x1 = End.x;
        var y1 = End.y;
        char sign = K > 0 ? '-' : '+';
        ExplicitEquation = $"y = {K.Round().ToString(CultureInfo.InvariantCulture)}x {sign.ToString(CultureInfo.InvariantCulture)}{System.Math.Abs((K * x1).Round() - (float) y1).ToString(CultureInfo.InvariantCulture)}";
        ImplictEquation = null;
    }

    private float GetAngle()
    {
        var angle = GetAngleRadian() * (float) (180/System.Math.PI);
        return angle;
    }

    private float GetAngleRadian()
    {
        var k = (float)K;
        var angle = (double) k.ArcTan();
        if (angle < 0)
        {
            //clockwise
            angle -= System.Math.PI;
            angle = -angle;     
        }

        return (float) angle;
    }
    /// <summary>
    /// Pulls the line onto the next quarter.
    /// </summary>
    /// <returns></returns>
    public Line2D ToEdgeAngleCast()
    {
        Line2D line2D = default;
        var magnitude = ((Start.x - End.x).Squared() + (Start.y - End.y).Squared()).SquareRoot();
        if (Angle is < 90 and > 0 or > 270)
        {
            line2D = new Line2D(Start, new Vector2D(Start.x - magnitude, Start.y));
        }
        else if (Angle.IsWithinMargin(90, 0.02f))
        {
            line2D = this;
        }
        else if (Angle.IsWithinMargin(270, 0.02f) || Angle is > 180 and < 270)
        {
            line2D = new Line2D(Start, new Vector2D(Start.x, Start.y + magnitude));
        }
        else if (Angle is > 90 and < 180)
        {
            line2D = new Line2D(Start, new Vector2D(Start.x + magnitude, Start.y));
        }
        return line2D;
    }

    public float GetY(float x)
    {
        return (float) (K * x - (K * (float) Start.x - (float) Start.y));
    }
    
    
    public LineEquationResults FindCircleEndPoint(float radius, float p, float q)
    {
        //(x-p)^2 + (y-q)^2 = radius^2
        //(x-p)^2 + (Kx-C-q)^2 = radius^2
        //(x^2 - 2(x*p) + p^2) + ((Kx)^2 - 2(Kx*(C-q)) + (C-q)^2 - radius^2 = 0
        var xSq = 0f;
        var x = 0f;
        var num = 0f;
        var c = -(K * (float) Start.x - (float) Start.y);
        
        
        xSq += 1;
        x -= 2f * (float) p;
        num +=  ((float) p).Squared() - ((float) radius).Squared();

        xSq += K.Squared();
        x += 2f * K * (c - (float) q);
        num += (c - q).Squared();

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