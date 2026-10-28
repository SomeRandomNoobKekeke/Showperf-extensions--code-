using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using CUILibs;
using System.Diagnostics;

namespace ShowperfExtensions;

public partial class MeasurementsPage : CUIPage
{
  public CaptureCore? Capture => Plugin.Capture;
  public Settings Settings { get; set; }

  public CUIFastTextDump Dump { get; }

  public static double TicksToMs = 1000.0 / Stopwatch.Frequency;
  public string ConverToMs(double t) => string.Format("{0:0.000000}", t * TicksToMs);

  private void RefreshData()
  {
    if (Capture is null)
    {
      Dump.Clear();
      return;
    }

    var sortedTicks = Capture.Update.TotalSlice.OrderByDescending(kvp => kvp.Value).ToList();

    double maxValue = sortedTicks.Count == 0 ? 0 : sortedTicks[0].Value;

    Dump.Lines = sortedTicks.Select(kvp => new FastTextDump.Line(
      $"{ConverToMs(kvp.Value / Capture.Update.SliceCount)}ms {kvp.Key}",
      Settings.Gradient((float)(kvp.Value / maxValue))
    )).ToList();

    // CUI.Logger.Log(Logger.Wrap.IEnumerable(lines.Select(l => l.ToString()), true));
  }

  public MeasurementsPage(Settings settings)
  {
    Settings = settings;

    this["layout"] = new CUIVerticalList() { Relative = new CUINullRect(0, 0, 1, 1) };
    this["layout"]["scroll wrapper"] = new CUIDefault.ListWithScrollbar()
    {
      Flex = 1,
      List = { BottomGap = ResizeHandle.DefaultSize.Y * 2 }
    };

    this["layout"]["scroll wrapper"]["list"]["dump"] = Dump = new CUIFastTextDump()
    {
      Offset = new Vector2(10, 10),
      Solid = true,
    };

    Capture!.Update.Rotated += RefreshData;
  }

}