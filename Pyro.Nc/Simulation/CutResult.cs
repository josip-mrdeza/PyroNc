using System;

namespace Pyro.Nc.Simulation
{
    public struct CutResult
    {
        public double TotalTime;
        public long TotalVerticesCut;
        public bool Threw;

        public CutResult(double totalTime, long totalVerticesCut)
        {
            TotalTime = totalTime;
            TotalVerticesCut = totalVerticesCut;
            Threw = false;
        }
        
        public CutResult(long totalTime, long totalVerticesCut, bool threw)
        {
            TotalTime = totalTime;
            TotalVerticesCut = totalVerticesCut;
            Threw = threw;
        }
    }
}