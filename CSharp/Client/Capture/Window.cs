using System;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using System.Text;

namespace ShowPerfExtensions
{
  public partial class Plugin : IAssemblyPlugin
  {
    public enum CaptureWindowMode { Sum, Mean, Spike }
    public enum SubType { Player, Outpost, OutpostModule, Wreck, BeaconStation, EnemySubmarine, Ruin, All }

    public class CaptureWindow : IDisposable
    {
      public bool Frozen = false;
      public bool Reseted;

      private int frames; public int Frames
      {
        get => frames;
        set
        {
          frames = Math.Max(1, value);
          Reset();
        }
      }

      public event Action<CaptureWindowMode> OnModeChanged;
      private CaptureWindowMode mode = CaptureWindowMode.Mean; public CaptureWindowMode Mode
      {
        get => mode;
        set
        {
          mode = value;
          Reset();
          OnModeChanged?.Invoke(mode);
        }
      }

      public event Action<SubType> OnCaptureFromChanged;

      private SubType captureFrom; public SubType CaptureFrom
      {
        get => captureFrom;
        set
        {
          captureFrom = value;
          Reset();
          OnCaptureFromChanged?.Invoke(captureFrom);
        }
      }

      public bool ShouldCapture(Entity e)
      {
        if (CaptureFrom == SubType.All) return true;
        if (e == null) return false;
        if (e.Submarine == null || e.Submarine.Info == null) return false;
        return (int)e.Submarine.Info.Type == (int)CaptureFrom;
      }

      public Slice FirstSlice;
      public Slice TotalTicks = new Slice();
      public Queue<Slice> PartialSums;

      public CaptureWindow(int frames = 100)
      {
        Frames = frames;
      }

      public void Rotate()
      {
        Reseted = false;
        if (Mode == CaptureWindowMode.Sum)
        {
          TotalTicks.Add(FirstSlice);
          FirstSlice.Clear();
        }

        if (Mode == CaptureWindowMode.Mean)
        {
          if (Frames == 1)
          {
            TotalTicks.Clear();
            TotalTicks.Add(FirstSlice);
            FirstSlice.Clear();
          }
          else
          {
            Slice lastSlice = PartialSums.Dequeue();

            TotalTicks.Add(FirstSlice);
            TotalTicks.Substract(lastSlice);

            lastSlice.Clear();
            FirstSlice = lastSlice;
            PartialSums.Enqueue(lastSlice);
          }
        }
      }

      public double LastUpdateTime;
      public void Update()
      {
        try
        {
          if (Frozen || GameMain.Instance.Paused) return;

          Rotate();
          LastUpdateTime = Timing.TotalTime;
        }
        catch (Exception e) { error(e); }
      }

      public void Reset()
      {
        if (Reseted) return;
        Reseted = true;

        PartialSums ??= new Queue<Slice>(Frames);
        PartialSums.Clear();
        TotalTicks.Clear();

        for (int i = 0; i < Frames - 1; i++)
        {
          PartialSums.Enqueue(new Slice());
        }

        FirstSlice = new Slice();
        PartialSums.Enqueue(FirstSlice);
      }

      public void EnsureCategory(int cat) => FirstSlice.EnsureCategory(cat);

      public void AddTicks(UpdateTicks t)
      {
        try
        {
          FirstSlice.Add(t);
        }
        catch (KeyNotFoundException e)
        {
          EnsureCategory(t.Category);
          error($"tried to add ticks to missing category with hash = {t.Category}");
        }
        catch (Exception e)
        {
          error(e.Message);
        }
      }

      public UpdateTicks GetTotal(int category, int id)
      {
        try
        {
          return Mode switch
          {
            CaptureWindowMode.Sum => TotalTicks[category][id],
            CaptureWindowMode.Mean => TotalTicks[category][id] / Frames
          };
        }
        catch (Exception e)
        {
          error(e);
          return new UpdateTicks(0, category, "[[not found]]");
        }
      }

      public void Dispose()
      {
        foreach (var s in PartialSums) s.Clear();
        PartialSums.Clear();
        TotalTicks.Clear();

        FirstSlice = null;
        PartialSums = null;
        TotalTicks = null;
      }
    }
  }
}