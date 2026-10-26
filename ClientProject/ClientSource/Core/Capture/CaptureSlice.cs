
namespace ShowperfExtensions;

public class CaptureSlice
{
  public long Total { get; private set; }
  public void AddTicks(string id, long value)
  {
    Ticks[id] += value;
    Total += value;
  }

  private Dictionary<string, long> Ticks { get; } = new();
}