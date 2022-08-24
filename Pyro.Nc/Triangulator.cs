using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pyro.Math;
using Pyro.Math.Geometry;
using UnityEngine;

namespace Pyro.Nc
{
    public class Triangulator
    {
        public Triangulator(ComplexShape shape)
        {
            Object = shape as PObject;
        }
        
        public Triangulator(Mesh mesh)
        {
            Mesh = mesh;
        }

        public async Task TriangulateMesh()
        {
            var trigs = await Triangulate();
            Debug.LogWarning("Completed");
            Mesh.triangles = trigs;
            Mesh.RecalculateBounds();
        }
        
        public async Task<Triangle[]> TriangulateAlt()
        {
            return (await Triangulate()).ToTriangles();
        }

        public async Task<int[]> Triangulate()
        {
            List<int> indices = new List<int>();
            int n = Points.Length;

            if (n < 3) return indices.ToArray();

            int[] V = new int[n];
            if (Area() > 0)
            {
                for (int v = 0; v < n; v++)
                {
                    V[v] = v;
                }
            }
            else
            {
                for (int v = 0; v < n; v++)
                {
                    V[v] = (n - 1) - v;
                }
            }

            int nv = n;
            int count = 2 * nv;
            for (int m = 0, v = nv - 1; nv > 2;)
            {
                if ((count--) <= 0)
                {
                    return indices.ToArray();
                }

                int u = v;
                if (nv <= u)
                {
                    u = 0;
                }
                v = u + 1;
                if (nv <= v)
                {
                    v = 0;
                }
                int w = v + 1;
                if (nv <= w)
                {
                    w = 0;
                }

                if (await Snip(u, v, w, nv, V))
                {
                    await Task.Yield();
                    int a, b, c, s, t;
                    a = V[u];
                    b = V[v];
                    c = V[w];
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);
                    m++;
                    for (s = v, t = v + 1; t < nv; s++, t++)
                    {
                        V[s] = V[t];
                    }
                    nv--;
                    count = 2 * nv;
                }
            }

            indices.Reverse();

            return indices.ToArray();
        }

        private float Area() 
        {
            int n = Points.Length;
            float A = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                Vector2 pval = Points[p];
                Vector2 qval = Points[q];
                A += pval.x * qval.y - qval.x * pval.y;
            }
            return (A * 0.5f);
        }
        
        private async Task<bool> Snip(int u, int v, int w, int n, int[] V)
        {
            int p;
            Vector2 A = Points[V[u]];
            Vector2 B = Points[V[v]];
            Vector2 C = Points[V[w]];
            if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
            {
                return false;
            }
            for (p = 0; p < n; p++)
            {
                await Task.Yield();
                if ((p == u) || (p == v) || (p == w))
                {
                    continue;
                }
                Vector2 P = Points[V[p]];
                if (InsideTriangle(A, B, C, P))
                {
                    return false;
                }
            }
            return true;
        }
        
        private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
        {
            float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
            float cCROSSap, bCROSScp, aCROSSbp;
         
            ax = C.x - B.x; ay = C.y - B.y;
            bx = A.x - C.x; by = A.y - C.y;
            cx = B.x - A.x; cy = B.y - A.y;
            apx = P.x - A.x; apy = P.y - A.y;
            bpx = P.x - B.x; bpy = P.y - B.y;
            cpx = P.x - C.x; cpy = P.y - C.y;
         
            aCROSSbp = ax * bpy - ay * bpx;
            cCROSSap = cx * apy - cy * apx;
            bCROSScp = bx * cpy - by * cpx;
            return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
        }
        
        public PObject Object;
        public Mesh Mesh;
        public Vector2[] Points
        {
            get => (Object is not null ? Object.Points.Select(x => new Vector2(x.x, x.z)) : Mesh.vertices.Select(x => new Vector2(x.x, x.z))).ToArray();
        }
    }
}