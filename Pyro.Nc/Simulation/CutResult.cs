namespace Pyro.Nc.Simulation
{
    public struct CutResult
    {
        public long TotalTime;
        public long TotalVerticesCut;

        public CutResult(long totalTime, long totalVerticesCut)
        {
            TotalTime = totalTime;
            TotalVerticesCut = totalVerticesCut;
        }
    }
}