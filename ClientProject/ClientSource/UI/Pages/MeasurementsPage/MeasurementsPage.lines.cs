using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using CUILibs;
using System.Diagnostics;
using Barotrauma;

namespace ShowperfExtensions;

public partial class MeasurementsPage : CUIPage
{
  public static double TicksToMs = 1000.0 / Stopwatch.Frequency;
  public string ConverToMs(double t) => string.Format("{0:0.000000}", t * TicksToMs);


  private List<KeyValuePair<Identifier, double>> SortedTicks = [];
  private double maxValue;

  protected override void UpdateRects()
  {
    base.UpdateRects();
    CullLines();
  }

  protected override void HandleOpen()
  {
    Capture.Window.Clear();
  }


  private void RefreshData()
  {
    if (Capture is null)
    {
      Dump.Clear();
      return;
    }

    SumTextBlock.Text = $"Total: {ConverToMs(Capture.Window.GetTotal())}ms";

    var values = Capture.Window.Values;

    SortedTicks = values.OrderByDescending(kvp => kvp.Value).ToList();

    maxValue = SortedTicks.Count == 0 ? 0 : SortedTicks[0].Value;

    CullLines();
  }

  private void CullLines()
  {
    var culledTicks = SortedTicks;

    double lineHeight = Dump.Shift.Y;
    int howManyLinesWillFitOntoScreen = (int)Math.Round(ScrollWrapper.Rect.Height / lineHeight);

    int topGap = Math.Max(howManyLinesWillFitOntoScreen, 5);
    int bottomGap = Math.Max(howManyLinesWillFitOntoScreen, 5);

    int skippedLines = 0;
    if (SortedTicks.Count > howManyLinesWillFitOntoScreen)
    {
      int spaceToScroll = SortedTicks.Count - howManyLinesWillFitOntoScreen;

      skippedLines = (int)Math.Round(ScrollWrapper.ScrollBar.Lambda * spaceToScroll);
      skippedLines = Math.Max(0, skippedLines - topGap);

      culledTicks = SortedTicks
        .Skip(skippedLines)
        .Take(howManyLinesWillFitOntoScreen + topGap + bottomGap)
        .ToList();
    }

    Dump.Lines = culledTicks.Select(kvp => new FastTextDump.Line(
      $"{ConverToMs(kvp.Value)}ms {kvp.Key}",
      Settings.Gradient((float)(kvp.Value / maxValue))
    )).ToList();

    Dump.Skip = skippedLines;


    Dump.Absolute = new CUINullRect(h: Dump.Shift.Y * SortedTicks.Count);
  }

}