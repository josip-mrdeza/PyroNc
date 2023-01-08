namespace Pyro.Nc.Simulation;

public struct CutInfo
{
    public int Index;
    public float Height;
    public bool IsOccupied;

    public CutInfo(int index, float height)
    {
        Index = index;
        Height = height;
        IsOccupied = true;
    }
}