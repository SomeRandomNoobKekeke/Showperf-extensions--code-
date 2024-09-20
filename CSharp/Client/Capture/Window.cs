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
  public partial class Mod : IAssemblyPlugin
  {
    public class CaptureWindow : IDisposable
    {
      public double FrameDuration;
      public bool Frozen = false;

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
      private bool accumulate; public bool Accumulate
      {
        get => accumulate;
        set
        {
          accumulate = value;
          Reset();
        }
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
        if (Accumulate)
        {
          TotalTicks.Add(FirstSlice);
          FirstSlice.Clear();
        }
        else
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
        catch (Exception e) { err(e); }
      }

      public void Reset()
      {
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

      public void AddTicks(UpdateTicks t)
      {
        try { FirstSlice.Add(t); }
        catch (Exception e) { err(e.Message); }
      }

      public UpdateTicks GetTotal(int id)
      {
        try
        {
          return Accumulate ? TotalTicks[id] : TotalTicks[id] / Frames * FPS;
        }
        catch (Exception e)
        {
          err(e);
          return new UpdateTicks("[[not found]]", 0);
        }
      }

      // public void debugCapacity()
      // {
      //   if (mod.debug)
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