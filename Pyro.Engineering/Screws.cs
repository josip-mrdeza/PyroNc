using System.Text;

namespace Pyro.Engineering
{
    public static class Screws
    {
        private static StringBuilder Builder = new StringBuilder();
        
        public struct Hardness
        {
            public float Value;
            
            public override string ToString()
            {
                Builder.Clear();
                Builder.Append("Sigma(L) = ").Append(Value);
                return Builder.ToString();
            }

            public static implicit operator float(Hardness hardness)
            {
                return hardness.Value;
            }
            
            public static implicit operator Hardness(float val)
            {
                var h = new Hardness
                {
                    Value = val
                };

                return h;
            }
        }
        
        public struct Dimensions
        {
            public float d;
            public float d1;
            public AverageDiameter d2;
            public float H;
            public CoreArea A1;

            public override string ToString()
            {
                Builder.Clear();
                Builder.Append("Screw M").Append(d);
                return Builder.ToString();
            }
        }

        public struct CoreArea
        {
            public float Value;
            
            public static implicit operator float(CoreArea hardness)
            {
                return hardness.Value;
            }
            
            public static implicit operator CoreArea(float val)
            {
                var h = new CoreArea
                {
                    Value = val
                };

                return h;
            }
        }

        public struct AverageDiameter
        {
            public float Value;
            
            public static implicit operator float(AverageDiameter hardness)
            {
                return hardness.Value;
            }
            
            public static implicit operator AverageDiameter(float val)
            {
                var h = new AverageDiameter
                {
                    Value = val
                };

                return h;
            }
        }

        public static void Main()
        {
            Dimensions c = new Dimensions();
            
        }
    }
}