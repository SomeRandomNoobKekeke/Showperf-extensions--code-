
using Barotrauma;
using CUILibs;

namespace ShowperfExtensions;

public class CaptureSlice : Dictionary<Identifier, double>
{
  public double Total { get; private set; }
  public double MaxValue { get; private set; }


  public void Clear()
  {
    base.Clear();
    Total = 0;
    MaxValue = 0;
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
    MaxValue = Math.Max(value, MaxValue);
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
    left.MaxValue = Math.Max(left.MaxValue, right.MaxValue);

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
          left[key] = result;
        }
      }
      else
      {
        left[key] = value;
      }
    }

    left.Total -= right.Total;
    left.MaxValue = Math.Min(left.MaxValue, right.MaxValue);

    return left;
  }


  public static CaptureSlice ReplaceWithMax(CaptureSlice left, CaptureSlice right)
  {
    foreach (var (key, value) in right)
    {
      if (left.TryGetValue(key, out double leftValue))
      {
        double result = Math.Max(leftValue, value);
        if (result <= 0)
        {
          left.Remove(key);
        }
        else
        {
          left[key] = result;
        }
      }
      else
      {
        left[key] = value;
      }
    }

    left.Total = Math.Max(left.Total, right.Total);
    left.MaxValue = Math.Max(left.MaxValue, right.MaxValue);

    return left;
  }

  public static CaptureSlice RemoveMatches(CaptureSlice left, CaptureSlice right)
  {
    foreach (var (key, value) in right)
    {
      if (left.TryGetValue(key, out double leftValue))
      {
        if (leftValue == value)
        {
          left.Remove(key);
        }
      }
    }

    if (left.Total == right.Total)
    {
      left.Total = 0;
    }

    if (left.MaxValue == right.MaxValue)
    {
      left.MaxValue = 0;
    }

    return left;
  }

  public override string ToString() => Logger.Wrap.IDictionary(this);

}