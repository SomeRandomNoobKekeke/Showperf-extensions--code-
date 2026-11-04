
using Barotrauma;
using CUILibs;

namespace ShowperfExtensions;

public class CaptureWindow
{

  public static int DefaultSliceCount = 100;
  public int SliceCount
  {
    get => Slices.Count;
    set
    {
      TotalSlice.Clear();
      ResetSliceQueue(value);
    }
  }

  private void ResetSliceQueue(int count)
  {
    Slices = new Queue<CaptureSlice>(count);
    for (int i = 0; i < count; i++)
    {
      Slices.Enqueue(new());
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

  public void Clear()
  {
    TotalSlice.Clear();
    ResetSliceQueue(SliceCount);
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

  private bool _Frozen; public bool Frozen
  {
    get => _Frozen;
    set
    {
      _Frozen = value;
      StagingSlice = new();
    }
  }

  public void Rotate()
  {
    if (Frozen) return;

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

