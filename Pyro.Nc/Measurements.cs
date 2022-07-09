using System.Diagnostics;

namespace Pyro.Nc{
  public readonly class Measurement{
    private Stopwatch time;
    public Measurement(){
    }
    public void Start(){
       time = Stopwatch.StartNew();
     }
    public void Stop() => time.Stop();

    public override string ToString(){
      return time.ElapsedMiliseconds;
    }
    }
  }
}
