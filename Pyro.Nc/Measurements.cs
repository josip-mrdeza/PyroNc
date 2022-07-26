using System.Diagnostics;

namespace Pyro.Nc
{
  public class Measurement
  {
    private Stopwatch time;

    public Measurement()
    {
    }

    public void Start()
    {
      time = Stopwatch.StartNew();
    }

    public void Stop() => time.Stop();

    public override string ToString()
    {
      return time.ElapsedMilliseconds.ToString();
    }
  }
}
