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
          foreach (CUIComponent c in Children) { PassLocked(c); }
        }
      }

      public void PassLocked(CUIComponent c)
      {
        c.ConsumeSwipe = !locked;
        c.ConsumeDragAndDrop = true;
        c.Draggable = !locked;
      }


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

#if DEBUG
        this["wrapper"] = new CUIHorizontalList()
        {
          Anchor = new Vector2(1, 0),
          Fixed = true,
          Unserializable = true,
          ZIndex = 100,
          FitContent = new CUIBool2(true, true),
        };

        this["wrapper"]["add"] = new CUIButton("add")
        {
          AddOnMouseDown = (e) =>
          {
            Locked = !Locked;
            this["wrapper"].Get<CUIButton>("add").Text = Locked ? "locked" : "unlocked";
          },
        };

        this["wrapper"]["save"] = new CUIButton("Save")
        {
          AddOnMouseDown = (e) => SaveToFile(Mod.ModDir + "/XML/CUICaptureMap.xml"),
        };
        this["wrapper"]["load"] = new CUIButton("Load")
        {
          AddOnMouseDown = (e) => Showperf.LoadMap(),
        };
#endif

        Locked = false;
        OnChildAdded += (c) => PassLocked(c);
      }
    }
  }
}