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
    public class CaptureWindow : IDisposable
    {

      public bool FreezeOnPause { get; set; }

      private int frames; public int Frames
      {
        get => frames;
        set
        {
          frames = Math.Max(1, value);
          Reset();
        }
      }

      public Slice FirstSlice;
      public Slice TotalTicks = new Slice();
      public Queue<Slice> PartialSums;

      public CaptureWindow(int frames = 100)
      {
        Frames = frames;
      }

      private bool Reseted;
      public void Rotate()
      {
        Reseted = false;
        if (Capture.Mode == CaptureMode.Sum)
        {
          TotalTicks.Add(FirstSlice);
          FirstSlice.Clear();
          return;
        }

        if (Capture.Mode == CaptureMode.Spike)
        {
          if (Frames == 1)
          {
            TotalTicks.ReplaceWithMax(FirstSlice);
          }
          else
          {
            Slice lastSlice = PartialSums.Dequeue();

            TotalTicks.RemoveMatches(lastSlice);
            TotalTicks.ReplaceWithMax(FirstSlice);

            lastSlice.Clear();
            FirstSlice = lastSlice;
            PartialSums.Enqueue(lastSlice);
          }

          return;
        }

        if (Capture.Mode == CaptureMode.Mean)
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
          return;
        }
      }

      public double LastUpdateTime;
      public void Update()
      {
        try
        {
          if (Capture.Frozen || (FreezeOnPause && GameMain.Instance.Paused)) return;

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
      public void EnsureCategory(CaptureState cs)
      {
        if (cs.IsActive) EnsureCategory(cs.ID.HashCode);
      }



      public void AddTicks(UpdateTicks t)
      {
        try
        {
          FirstSlice.Add(t);
        }
        catch (KeyNotFoundException e)
        {
          EnsureCategory(t.Category);
          FirstSlice.Add(t);
          error($"tried to add ticks to missing category {CaptureState.FromHash[t.Category]}");
        }
        catch (Exception e)
        {
          error(e.Message);
        }
      }


      public void AddTicks(double ticks, CaptureState cs, string name) => AddTicks(ticks, cs, name, name.GetHashCode());
      public void AddTicks(double ticks, CaptureState cs, Identifier id) => AddTicks(ticks, cs, id.Value, id.HashCode);
      public void AddTicks(double ticks, CaptureState cs, string name, int hash)
      {
        try
        {
          if (!cs.IsActive) return;
          FirstSlice.Add(new UpdateTicks(ticks, cs.ID.HashCode, name, hash));
        }
        catch (KeyNotFoundException e)
        {
          EnsureCategory(cs);
          FirstSlice.Add(new UpdateTicks(ticks, cs.ID.HashCode, name, hash));
          error($"tried to add ticks to missing category {cs}");
        }
        catch (Exception e)
        {
          error(e.Message);
        }
      }

      public void AddTicksOnce(UpdateTicks t)
      {
        EnsureCategory(t.Category);
        AddTicks(t);
      }
      public void AddTicksOnce(double ticks, CaptureState cs, string name) => AddTicksOnce(ticks, cs, name, name.GetHashCode());
      public void AddTicksOnce(double ticks, CaptureState cs, Identifier id) => AddTicksOnce(ticks, cs, id.Value, id.HashCode);
      public void AddTicksOnce(double ticks, CaptureState cs, string name, int hash)
      {
        if (!cs.IsActive) return;
        EnsureCategory(cs);
        AddTicks(new UpdateTicks(ticks, cs.ID.HashCode, name, hash));
      }




      public UpdateTicks GetTotal(int category, int id)
      {
        try
        {
          return Capture.Mode switch
          {
            CaptureMode.Sum => TotalTicks[category][id],
            CaptureMode.Spike => TotalTicks[category][id],
            CaptureMode.Mean => TotalTicks[category][id] / (Frames - 1) // TODO why i need -1 here?
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