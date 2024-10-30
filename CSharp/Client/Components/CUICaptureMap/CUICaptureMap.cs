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
using System.IO;

namespace ShowPerfExtensions
{
  public partial class Plugin : IAssemblyPlugin
  {
    public partial class CUICaptureMap : CUIMap
    {


      private bool locked; public bool Locked
      {
        get => locked;
        set
        {
          locked = value;
          if (locked)
          {
            SaveButton.RemoveSelf();
            LoadButton.RemoveSelf();
          }
          else
          {
            if (LoadButton.Parent == null) this["wrapper"].Prepend(LoadButton);
            if (SaveButton.Parent == null) this["wrapper"].Prepend(SaveButton);
          }

          LockButton.Text = locked ? "Locked" : "Unlocked";

          foreach (CUIComponent c in Children) { PassLocked(c); }
        }
      }

      public void PassLocked(CUIComponent c)
      {
        if (c is MapGroup || c is MapButton)
        {
          c.ConsumeSwipe = !locked;
          c.ConsumeDragAndDrop = true;
          c.Draggable = !locked;
        }
      }

      public CUIButton LockButton;
      public CUIButton SaveButton;
      public CUIButton LoadButton;


      //Note: this method is temporary
      public void AddStuff()
      {
        MapGroup g = new MapGroup()
        {
          Absolute = new CUINullRect(x: 50, y: 200),
          Caption = "Group",
        };
        g.Header.Debug = true;

        g.Add(new CUIButton("bebeeb"));
        g.Add(new CUIButton("bebeeb"));
        g.Add(new CUIButton("bebeeb") { });
        g.Add(new CUIButton("bebeeb"));

        Add(g);
      }


      public CUICaptureMap()
      {
        // BackgroundColor = Color.Transparent;
        // BorderColor = Color.Transparent;
        OnDClick += (e) => SetChildrenOffset(new CUI3DOffset(0, 0, 1));

        Append(new CUITextBlock("Client")
        {
          Fixed = true,
          Unserializable = true,
        });


        this["wrapper"] = new CUIHorizontalList()
        {
          Anchor = new Vector2(1, 0),
          Fixed = true,
          Unserializable = true,
          ZIndex = 100,
          FitContent = new CUIBool2(true, true),
        };


        SaveButton = new CUIButton("Save")
        {
          AddOnMouseDown = (e) => SaveToFile(Mod.ModDir + "/XML/CUICaptureMap.xml"),
        };

        LoadButton = new CUIButton("Load")
        {
          AddOnMouseDown = (e) => Showperf.LoadMap(),
        };

        this["wrapper"]["lock"] = LockButton = new CUIButton("Unlocked")
        {
          Absolute = new CUINullRect(w: 70),
          InactiveColor = CUIPallete.Default.Tertiary.Off,
          MouseOverColor = CUIPallete.Default.Tertiary.OffHover,
          MousePressedColor = CUIPallete.Default.Tertiary.On,
          BorderColor = CUIPallete.Default.Tertiary.Border,
          DisabledColor = CUIPallete.Default.Tertiary.Disabled,
          AddOnMouseDown = (e) =>
          {
            Locked = !Locked;
          },
        };


        Locked = false;
        OnChildAdded += (c) => PassLocked(c);
      }
    }
  }
}