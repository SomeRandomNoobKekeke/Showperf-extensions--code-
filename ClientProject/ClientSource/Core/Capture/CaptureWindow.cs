
using Barotrauma;
using CUILibs;

namespace ShowperfExtensions;

public class CaptureWindow
{

  public enum CaptureMode { Average, Sum, Spike }

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


  public string IncludeFilter { get; set; }
  public string ExcludeFilter { get; set; }


  public void AddTicks(string id, double value) => AddTicks(id.ToIdentifier(), value);
  public void AddTicks(Identifier id, double value)
  {
    if (!string.IsNullOrEmpty(ExcludeFilter) && id.Value.Contains(ExcludeFilter)) return;
    if (!string.IsNullOrEmpty(IncludeFilter) && !id.Value.Contains(IncludeFilter)) return;

    StagingSlice.AddTicks(id, value);
  }

  public double MaxValue => TotalSlice.MaxValue;

  public double GetTotal() => Mode switch
  {
    CaptureMode.Average => TotalSlice.Total / SliceCount,
    CaptureMode.Sum => TotalSlice.Total,
    CaptureMode.Spike => TotalSlice.Total,
  };

  public void Clear()
  {
    TotalSlice.Clear();
    ResetSliceQueue(SliceCount);
  }

  public IEnumerable<KeyValuePair<Identifier, double>> Values
  {
    get => Mode switch
    {
      CaptureMode.Average => TotalSlice.Select(kvp => new KeyValuePair<Identifier, double>(
        kvp.Key, kvp.Value / SliceCount
      )),
      CaptureMode.Sum => TotalSlice,
      CaptureMode.Spike => TotalSlice,
    };
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

  private CaptureMode _Mode; public CaptureMode Mode
  {
    get => _Mode;
    set
    {
      _Mode = value;
      Clear();
    }
  }

  public void Rotate()
  {
    if (Frozen) return;



    switch (Mode)
    {
      case CaptureMode.Average:
        TotalSlice += StagingSlice;

        Slices.Enqueue(StagingSlice);
        TotalSlice -= Slices.Dequeue();

        StagingSlice = new();
        break;

      case CaptureMode.Sum:
        TotalSlice += StagingSlice;
        StagingSlice = new();
        break;

      case CaptureMode.Spike:
        Slices.Enqueue(StagingSlice);

        CaptureSlice.RemoveMatches(TotalSlice, Slices.Dequeue());
        CaptureSlice.ReplaceWithMax(TotalSlice, StagingSlice);

        StagingSlice = new();
        break;
    }

    Rotated?.Invoke();
  }

  public CaptureWindow()
  {
    SliceCount = DefaultSliceCount;
  }

}

