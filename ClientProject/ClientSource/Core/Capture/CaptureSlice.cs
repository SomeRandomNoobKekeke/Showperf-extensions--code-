
using CUILibs;

namespace ShowperfExtensions;

public class CaptureSlice : Dictionary<string, long>
{
  public long Total { get; private set; }
  public void AddTicks(string id, long value)
  {
    if (this.TryGetValue(id, out long prevValue))
    {
      this[id] += value;
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
      if (left.TryGetValue(key, out long leftValue))
      {
        left[key] = leftValue + value;
      }
      else
      {
        left[key] = value;
      }
    }

    return left;
  }

  public static CaptureSlice operator -(CaptureSlice left, CaptureSlice right)
  {
    foreach (var (key, value) in right)
    {
      if (left.TryGetValue(key, out long leftValue))
      {
        long result = leftValue - value;
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

    return left;
  }

  public override string ToString() => Logger.Wrap.IDictionary(this);

}