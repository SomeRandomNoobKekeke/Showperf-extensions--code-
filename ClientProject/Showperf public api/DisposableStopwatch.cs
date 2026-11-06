using Barotrauma;
using ShowperfExtensions;
using CUILibs;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace ShowperfPatches;


/// <summary>
/// it looks like Stopwatch but implements IDisposable
/// So you can use it in using() blocks
/// </summary>
public class DisposableStopwatch : IDisposable
{
  public Stopwatch Watch { get; } = new();

  /// <summary>
  /// Instead of checking if(ok) watch.Capture() you can set watch.ShouldCapture = ok  
  /// And then all watch.Capture() calls will check that internally
  /// </summary>
  public bool ShouldCapture { get; set; } = true;

  /// <summary>
  /// Sends ticks to showperf if ShouldCapture == true
  /// </summary>
  public void Capture(string id)
  {
    if (ShouldCapture) ShowperfExtensions.Capture.Ticks(id, this);
  }

  /// <summary>
  /// Sends ticks to showperf if ShouldCapture == true
  /// </summary>
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

  /// <summary>
  /// Returns self
  /// </summary>
  public DisposableStopwatch Start()
  {
    Watch.Start();
    return this;
  }

  /// <summary>
  /// Returns self
  /// </summary>
  public DisposableStopwatch Stop()
  {
    Watch.Stop();
    return this;
  }

  /// <summary>
  /// Returns self
  /// </summary>
  public DisposableStopwatch Reset()
  {
    Watch.Reset();
    return this;
  }

  /// <summary>
  /// Returns self
  /// </summary>
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