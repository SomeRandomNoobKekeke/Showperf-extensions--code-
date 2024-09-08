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
      public Stopwatch stopwatch;


      public double FrameDuration;
      public double LastUpdateTime;
      public bool Frozen = false;
      public int frames;
      public int Frames
      {
        get { return frames; }
        set
        {
          frames = Math.Max(1, value);
          fps = frames / duration;
          FrameDuration = duration / frames;
          Reset();
        }
      }
      private double duration;
      public double Duration
      {
        get { return duration; }
        set
        {
          duration = Math.Max(0.1, value);
          frames = Math.Max(1, (int)Math.Ceiling(duration / FrameDuration));
          Reset();
        }
      }

      private double fps;
      public double FPS
      {
        get { return fps; }
        set
        {
          fps = Math.Min(Math.Max(1, value), 60);
          FrameDuration = 1.0 / fps;
          frames = Math.Max(1, (int)Math.Ceiling(duration / FrameDuration));
          Reset();
        }
      }



      public bool accumulate = false;
      public bool Accumulate
      {
        get => accumulate;
        set
        {
          accumulate = value;
          Reset();
        }
      }
      public Slice firstSlice;

      public Slice TotalTicks = new Slice();

      public Queue<Slice> partialSums;


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
          TotalTicks.Add(firstSlice);
          firstSlice.Clear();
        }
        else
        {
          if (Frames == 1)
          {
            TotalTicks.Clear();
            TotalTicks.Add(firstSlice);
            firstSlice.Clear();
          }
          else
          {
            Slice lastSlice = partialSums.Dequeue();

            TotalTicks.Add(firstSlice);
            TotalTicks.Substract(lastSlice);

            lastSlice.Clear();
            firstSlice = lastSlice;
            partialSums.Enqueue(lastSlice);
          }
        }

        //debugCapacity();
      }

      public bool ShouldUpdate => Timing.TotalTime - LastUpdateTime > FrameDuration;

      public void Update()
      {
        if (Frozen || GameMain.Instance.Paused) return;

        while (ShouldUpdate)
        {
          Rotate();
          LastUpdateTime += FrameDuration;
        }
      }

      public void Reset()
      {
        partialSums ??= new Queue<Slice>(frames);
        partialSums.Clear();
        TotalTicks.Clear();

        for (int i = 0; i < frames - 1; i++)
        {
          partialSums.Enqueue(new Slice());
        }

        firstSlice = new Slice();
        partialSums.Enqueue(firstSlice);

        info($"Reset| fps:{fps} duration:{duration} partialSums.Count: {partialSums.Count}");
      }

      public void ensureCategory(CaptureCategory cat)
      {
        if (!firstSlice.Categories.ContainsKey(cat)) firstSlice[cat] = new Dictionary<int, UpdateTicks>();
      }

      public void tryAddTicks(Identifier id, CaptureCategory cat, double ticks) => tryAddTicks(id.HashCode, id.Value, cat, ticks);

      public void tryAddTicks(string id, CaptureCategory cat, double ticks) => tryAddTicks(id.GetHashCode(), id, cat, ticks);


      public void tryAddTicks(int id, string name, CaptureCategory cat, double ticks)
      {
        try
        {
          if (firstSlice[cat].ContainsKey(id))
          {
            firstSlice[cat][id] += ticks;
          }
          else
          {
            firstSlice[cat][id] = new UpdateTicks(name, ticks);
          }
        }
        catch (KeyNotFoundException e)
        {
          ensureCategory(cat);
          err(e.Message);
        }
        catch (Exception e)
        {
          err(e.Message);
        }
      }

      public UpdateTicks GetTotal(CaptureCategory cat, int id)
      {
        try
        {
          return Accumulate ? TotalTicks[cat][id] : TotalTicks[cat][id] / Frames * FPS;
        }
        catch (Exception e)
        {
          err(e);
          return new UpdateTicks("[[not found]]", 0);
        }
      }

      public void debugCapacity()
      {
        if (mod.debug)
        {
          StringBuilder sb = new StringBuilder();
          string s = "";
          foreach (Slice slice in partialSums)
          {

            int sum = 0;
            foreach (var cat in slice.Categories)
            {
              sum += cat.Value.Count;
            }

            sb.Append($"{sum}|");
          }

          log(sb.ToString());
        }
      }


      public void Dispose()
      {
        foreach (var s in partialSums) s.Clear();
        partialSums.Clear();
        TotalTicks.Clear();

        firstSlice = null;
        partialSums = null;
        TotalTicks = null;
      }
    }
  }
}