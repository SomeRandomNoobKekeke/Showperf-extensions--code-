
using Barotrauma;
using CUILibs;

namespace ShowperfExtensions;

public class CaptureWindow
{

  public static int DefaultSliceCount = 500;
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

  public CaptureSlice StagingSlice { get; private set; } = new();
  public Queue<CaptureSlice> Slices { get; private set; } = [];
  public CaptureSlice TotalSlice { get; private set; } = new();

  public void AddTicks(string id, double value) => AddTicks(id.ToIdentifier(), value);
  public void AddTicks(Identifier id, double value)
  {
    StagingSlice.AddTicks(id, value);
  }

  public double GetTotal()
  {
    return TotalSlice.Total / SliceCount;
  }

  public IEnumerable<KeyValuePair<Identifier, double>> Values
  {
    get
    {
      return TotalSlice.Select(kvp => new KeyValuePair<Identifier, double>(
        kvp.Key, kvp.Value / SliceCount
      ));
    }
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
    SliceCount = DefaultSliceCount;
  }

}

