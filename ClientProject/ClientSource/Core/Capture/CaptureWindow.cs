
using Barotrauma;
using CUILibs;

namespace ShowperfExtensions;

public class CaptureWindow
{

  public enum CaptureMode { Average, Sum, Spike }

  public static int DefaultFrameCount = 100;
  public int FrameCount
  {
    get => Frames.Count;
    set
    {
      TotalFrame.Clear();
      ResetFrameQueue(value);
    }
  }

  private void ResetFrameQueue(int count)
  {
    Frames = new Queue<CaptureFrame>(count);
    for (int i = 0; i < count; i++)
    {
      Frames.Enqueue(new());
    }
  }

  public CaptureFrame StagingFrame { get; private set; } = new();
  public Queue<CaptureFrame> Frames { get; private set; } = [];
  public CaptureFrame TotalFrame { get; private set; } = new();


  public string IncludeFilter { get; set; }
  public string ExcludeFilter { get; set; }


  public void AddTicks(string id, double value) => AddTicks(id.ToIdentifier(), value);
  public void AddTicks(Identifier id, double value)
  {
    if (!string.IsNullOrEmpty(ExcludeFilter) && id.Value.Contains(ExcludeFilter)) return;
    if (!string.IsNullOrEmpty(IncludeFilter) && !id.Value.Contains(IncludeFilter)) return;

    StagingFrame.AddTicks(id, value);
  }

  public double MaxValue => TotalFrame.MaxValue;

  public double GetTotal() => Mode switch
  {
    CaptureMode.Average => TotalFrame.Total / FrameCount,
    CaptureMode.Sum => TotalFrame.Total,
    CaptureMode.Spike => TotalFrame.Total,
  };

  public void Clear()
  {
    TotalFrame.Clear();
    ResetFrameQueue(FrameCount);
  }

  public IEnumerable<KeyValuePair<Identifier, double>> Values
  {
    get => Mode switch
    {
      CaptureMode.Average => TotalFrame.Select(kvp => new KeyValuePair<Identifier, double>(
        kvp.Key, kvp.Value / FrameCount
      )),
      CaptureMode.Sum => TotalFrame,
      CaptureMode.Spike => TotalFrame,
    };
  }

  public event Action? Rotated;

  private bool _Frozen; public bool Frozen
  {
    get => _Frozen;
    set
    {
      _Frozen = value;
      StagingFrame = new();
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
        TotalFrame += StagingFrame;

        Frames.Enqueue(StagingFrame);
        TotalFrame -= Frames.Dequeue();

        StagingFrame = new();
        break;

      case CaptureMode.Sum:
        TotalFrame += StagingFrame;
        StagingFrame = new();
        break;

      case CaptureMode.Spike:
        Frames.Enqueue(StagingFrame);

        CaptureFrame.RemoveMatches(TotalFrame, Frames.Dequeue());
        CaptureFrame.ReplaceWithMax(TotalFrame, StagingFrame);

        StagingFrame = new();
        break;
    }

    Rotated?.Invoke();
  }

  public CaptureWindow()
  {
    FrameCount = DefaultFrameCount;
  }

}

