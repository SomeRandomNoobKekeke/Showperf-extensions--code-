//#define DEBUG

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
      public MapBend(int x, int y, bool disabled = true) : base()
      {

#if DEBUG
        Absolute = new CUINullRect(x: x, y: y, w: 5, h: 5);
        BackgroundColor = Color.Lime;
        BorderColor = Color.Transparent;
#else
        Absolute = new CUINullRect(x: x, y: y);
        BorderColor = Color.Transparent;
        Revealed = false;
#endif

        Disabled = disabled;
        ConsumeSwipe = true;
#if DEBUG
        Draggable = true;
        OnDrag += (x, y) => Info(Absolute.Position);
#endif
      }
    }

    public class MapButton : CUIToggleButton
    {
      public static Dictionary<CaptureState, MapButton> Buttons = new Dictionary<CaptureState, MapButton>();
      public CaptureState CState;

      public MapButton(int x, int y, string name, CaptureState cs = null) : base(name)
      {
        Absolute = new CUINullRect(x: x, y: y);

        Padding = new Vector2(2, 0);
        TextScale = 0.8f;
        CState = cs;
        ConsumeSwipe = false;
#if DEBUG
        Draggable = true;
        OnDrag += (x, y) => Info(Absolute.Position);
#endif
        if (cs != null)
        {
          State = cs.IsActive;
          OnStateChange += (state) => Showperf.OnMapButtonClicked(this);
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