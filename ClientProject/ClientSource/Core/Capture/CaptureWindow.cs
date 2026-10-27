
using CUILibs;

namespace ShowperfExtensions;

public class CaptureWindow
{

  public int SliceCount
  {
    get => Slices.Count;
    set
    {
      Slices = new Queue<CaptureSlice>(value);
      for (int i = 0; i < value; i++)
      {
        Slices.Enqueue(new());
      }
    }
  }

  private CaptureSlice StagingSlice = new();
  public Queue<CaptureSlice> Slices { get; private set; } = [];
  public CaptureSlice TotalSlice { get; private set; } = new();

  public void AddTicks(string id, long value)
  {
    StagingSlice.AddTicks(id, value);
  }

  public event Action? Rotated;

  public void Rotate()
  {
    TotalSlice += StagingSlice;


    Slices.Enqueue(StagingSlice);

    CaptureSlice last = Slices.Dequeue();
    TotalSlice -= last;

    StagingSlice = new();
    Rotated?.Invoke();
  }

  public CaptureWindow()
  {
    SliceCount = 100;
  }

}

