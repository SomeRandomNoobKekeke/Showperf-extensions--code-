using Barotrauma;
using ShowperfExtensions;
using CUILibs;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace ShowperfPatches;

public class DisposableStopwatch : IDisposable
{
  public Stopwatch Watch { get; } = new();

  public bool ShouldCapture { get; set; } = true;

  public void Capture(string id)
  {
    if (ShouldCapture) ShowperfExtensions.Capture.Ticks(id, this);
  }
  public void Capture(Identifier id)
  {
    if (ShouldCapture) ShowperfExtensions.Capture.Ticks(id, this);
  }


  public static long Frequency => Stopwatch.Frequency;
  public static bool IsHighResolution => Stopwatch.IsHighResolution;
  public static long GetTimestamp() => Stopwatch.GetTimestamp();
  public static TimeSpan GetElapsedTime(long startingTimestamp)
    => Stopwatch.GetElapsedTime(startingTimestamp);
  public static TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp)
    => Stopwatch.GetElapsedTime(startingTimestamp, endingTimestamp);

  public static DisposableStopwatch StartNew()
  {
    DisposableStopwatch s = new();
    s.Start();
    return s;
  }

  public DisposableStopwatch Start()
  {
    Watch.Start();
    return this;
  }
  public DisposableStopwatch Stop()
  {
    Watch.Stop();
    return this;
  }
  public DisposableStopwatch Reset()
  {
    Watch.Reset();
    return this;
  }
  public DisposableStopwatch Restart()
  {
    Watch.Restart();
    return this;
  }

  public override string ToString() => Watch.ToString();

  public bool IsRunning => Watch.IsRunning;
  public TimeSpan Elapsed => Watch.Elapsed;
  public long ElapsedMilliseconds => Watch.ElapsedMilliseconds;
  public long ElapsedTicks => Watch.ElapsedTicks;

  public void Dispose() => Stop();
}