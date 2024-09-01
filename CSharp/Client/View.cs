using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShowPerfExtensions
{
  public partial class Mod : IAssemblyPlugin
  {
    public class WindowView : IDisposable
    {
      // only one component for now, it could be dict in the future
      public GUIList ListGUIComponent;

      //public double FrameDuration = 1.0 / 30.0;
      public double LastUpdateTime;
      //public double FPS { set => FrameDuration = 1.0 / value; }

      public HashSet<string> Tracked;
      public int lastMWScroll;

      public bool ShowInMs = true;
      public string UnitsName { get => ShowInMs ? "ms" : "ticks"; }
      public string ConverToUnits(double t) => ShowInMs ?
      String.Format("{0:0.000000}", t * TicksToMs) :
      String.Format("{0:000000}", t);

      // this is for showperf_track hints
      public string[] getAllIds()
      {
        return ListGUIComponent.Values.Select(t => t.ID).ToArray();
      }

      public WindowView()
      {
        ListGUIComponent = new GUIList();
        Tracked = new HashSet<string>();
      }

      public void Clear() => ListGUIComponent.Clear();

      public bool ShouldUpdate => Timing.TotalTime - LastUpdateTime > Window.FrameDuration;
      public void Update()
      {
        if (Window.Frozen || GameMain.Instance.Paused) return;
        if (ShouldUpdate)
        {
          ListGUIComponent.Update();

          LastUpdateTime = Timing.TotalTime;
        }
      }

      public void UpdateScroll()
      {
        try
        {
          if (PlayerInput.IsShiftDown() && PlayerInput.IsAltDown())
          {
            double delta = (PlayerInput.mouseState.ScrollWheelValue - lastMWScroll) / 80.0;

            ListGUIComponent.ListShift -= delta;
          }
          lastMWScroll = PlayerInput.mouseState.ScrollWheelValue;
        }
        catch (Exception e)
        {
          err(e);
        }
      }

      public void Draw(SpriteBatch spriteBatch) => ListGUIComponent.Draw(spriteBatch);

      public void SetCategory(ShowperfCategory category)
      {
        ListGUIComponent.Clear();
        ListGUIComponent.Caption = CategoryNames[category];
      }

      public void Dispose()
      {
        ListGUIComponent.Clear();
        ListGUIComponent = null;
        Tracked.Clear();
        Tracked = null;
      }
    }
  }
}