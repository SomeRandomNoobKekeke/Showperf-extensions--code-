
using Barotrauma;
using CUILibs;

namespace ShowperfExtensions;

public class CaptureSlice : Dictionary<Identifier, double>
{
  public double Total { get; private set; }

  public void Clear()
  {
    base.Clear();
    Total = 0;
  }

  public void AddTicks(Identifier id, double value)
  {
    if (this.TryGetValue(id, out double prevValue))
    {
      this[id] = prevValue + value;
    }
    else
    {
      this[id] = value;
    }

    Total += value;
  }

  public static CaptureSlice operator +(CaptureSlice left, CaptureSlice right)
  {
    foreach (var (key, value) in right)
    {
      if (left.TryGetValue(key, out double leftValue))
      {
        left[key] = leftValue + value;
      }
      else
      {
        left[key] = value;
      }
    }

    left.Total += right.Total;

    return left;
  }

  public static CaptureSlice operator -(CaptureSlice left, CaptureSlice right)
  {
    foreach (var (key, value) in right)
    {
      if (left.TryGetValue(key, out double leftValue))
      {
        double result = leftValue - value;
        if (result <= 0)
        {
          left.Remove(key);
        }
        else
        {
          left[key] = leftValue - value;
        }
      }
      else
      {
        left[key] = value;
      }
    }

    left.Total -= right.Total;

    return left;
  }

  public override string ToString() => Logger.Wrap.IDictionary(this);

}