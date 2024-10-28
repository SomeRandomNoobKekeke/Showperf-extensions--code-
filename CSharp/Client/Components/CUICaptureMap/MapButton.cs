#define DEBUG

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using HarmonyLib;
using CrabUI;

namespace ShowPerfExtensions
{
  public partial class Plugin : IAssemblyPlugin
  {
    public class MapBend : CUIComponent
    {
      public MapBend() : base()
      {
        ConsumeSwipe = true;
#if DEBUG
        BackgroundColor = Color.Lime;
        BorderColor = Color.Transparent;

        Draggable = true;
#else
        BorderColor = Color.Transparent;
        Revealed = false;
#endif
      }
      public MapBend(int x, int y, bool disabled = true) : this()
      {
        Disabled = disabled;

#if DEBUG
        Absolute = new CUINullRect(x: x, y: y, w: 5, h: 5);
#else
        Absolute = new CUINullRect(x: x, y: y);
#endif
      }
    }

    public class MapButton : CUIToggleButton
    {
      public static Dictionary<CaptureState, MapButton> Buttons = new Dictionary<CaptureState, MapButton>();
      [CUISerializable] public CaptureState CState { get; set; }
      public MapButton() : base()
      {
        Padding = new Vector2(2, 0);
        TextScale = 0.8f;
        ConsumeSwipe = false;
        //TODO don't depend on global static vars
        OnStateChange += (state) => Showperf.OnMapButtonClicked(this);

#if DEBUG
        Draggable = true;
        ConsumeSwipe = true;
#endif
      }
      public MapButton(int x, int y, string name, CaptureState cs = null) : this()
      {
        Absolute = new CUINullRect(x: x, y: y);
        CState = cs;

        OnText = name;
        OffText = name;
        Text = name;

        if (cs != null)
        {
          State = cs.IsActive;
          Buttons[cs] = this;
        }
        else
        {
          Disabled = true;
        }
      }
    }
  }
}