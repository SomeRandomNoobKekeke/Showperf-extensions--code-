
namespace ShowperfExtensions;

public class CaptureWindow
{

  public int SliceCount
  {
    get => Slices.Count;
    set => Slices = new Queue<CaptureSlice>(value);
  }

  private CaptureSlice StagingSlice = new();
  public Queue<CaptureSlice> Slices { get; private set; } = new Queue<CaptureSlice>(100);
  public long Total { get; private set; }

  public void AddTicks(string id, long value)
  {
    StagingSlice.AddTicks(id, value);
  }

  public void Rotate()
  {
    Total += StagingSlice.Total;
    Slices.Enqueue(StagingSlice);

    CaptureSlice last = Slices.Dequeue();
    Total -= last.Total;

    StagingSlice = new();
  }

}

