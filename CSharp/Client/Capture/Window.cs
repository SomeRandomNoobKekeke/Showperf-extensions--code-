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
      public double FrameDuration;
      public bool Frozen = false;
      public bool Reseted;

      private int frames; public int Frames
      {
        get => frames;
        set
        {
          frames = Math.Max(1, value);
          fps = frames / duration;
          FrameDuration = duration / frames;
          Reset();
        }
      }

      private double duration; public double Duration
      {
        get => duration;
        set
        {
          duration = Math.Max(0.1, value);
          frames = Math.Max(1, (int)Math.Ceiling(duration / FrameDuration));
          Reset();
        }
      }
      private double fps; public double FPS
      {
        get => fps;
        set
        {
          fps = Math.Min(Math.Max(1, value), 60);
          FrameDuration = 1.0 / fps;
          frames = Math.Max(1, (int)Math.Ceiling(duration / FrameDuration));
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

      public CaptureWindow(double duration = 3, int fps = 30)
      {
        this.duration = duration;
        this.fps = fps;

        FrameDuration = 1.0 / fps;
        frames = Math.Max(1, (int)Math.Ceiling(duration / FrameDuration));

        Reset();
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
      public bool ShouldUpdate => Timing.TotalTime - LastUpdateTime > FrameDuration;

      public void Update()
      {
        try
        {
          if (Frozen || GameMain.Instance.Paused) return;

          while (ShouldUpdate)
          {
            Rotate();
            LastUpdateTime += FrameDuration;
          }
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

        info($"Reset| fps:{fps} duration:{duration} PartialSums.Count: {PartialSums.Count}");
      }

      public void EnsureCategory(int cat) => FirstSlice.EnsureCategory(cat);
      public void EnsureCategory(CName cat) => FirstSlice.EnsureCategory(cat);

      public void AddTicks(UpdateTicks t)
      {
        try
        {
          FirstSlice.Add(t);
        }
        catch (KeyNotFoundException e)
        {
          EnsureCategory(t.Category);
          error($"tried to add ticks to missing category {(CName)t.Category}");
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
            CaptureWindowMode.Mean => TotalTicks[category][id] / Frames * FPS
          };
        }
        catch (Exception e)
        {
          error(e);
          return new UpdateTicks(0, category, "[[not found]]");
        }
      }

      // public void debugCapacity()
      // {
      //   if (Mod.Debug)
      //   {
      //     StringBuilder sb = new StringBuilder();
      //     string s = "";
      //     foreach (Slice slice in PartialSums)
      //     {

      //       int sum = 0;
      //       foreach (var cat in slice.Categories)
      //       {
      //         sum += cat.Value.Count;
      //       }

      //       sb.Append($"{sum}|");
      //     }

      //     log(sb.ToString());
      //   }
      // }

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