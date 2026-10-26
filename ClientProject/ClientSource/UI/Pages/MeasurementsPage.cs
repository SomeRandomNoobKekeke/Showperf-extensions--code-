using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using CUILibs;

namespace ShowperfExtensions;

public partial class MeasurementsPage : CUIPage
{
  public Capture? Capture => Plugin.Capture;

  public CUIFastTextDump Dump { get; }

  private void RefreshData()
  {
    if (Capture is null)
    {
      Dump.Clear();
      return;
    }

    List<FastTextDump.Line> lines = [];
    foreach (var (key, value) in Capture.Update.TotalSlice)
    {
      lines.Add(new FastTextDump.Line($"{value / Capture.Update.SliceCount} ticks {key}", Color.White));
    }
    Dump.Lines = lines;

    // CUI.Logger.Log(Logger.Wrap.IEnumerable(lines.Select(l => l.ToString()), true));
  }

  public MeasurementsPage()
  {
    this["layout"] = new CUIVerticalList() { Relative = new CUINullRect(0, 0, 1, 1) };
    this["layout"]["scroll wrapper"] = new CUIDefault.ListWithScrollbar() { Flex = 1 };
    this["layout"]["scroll wrapper"]["list"]["dump"] = Dump = new CUIFastTextDump()
    {
      Solid = true,
    };

    Capture!.Update.Rotated += RefreshData;
  }

}