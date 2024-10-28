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

        Add("Group", g);
      }


      public CUICaptureMap()
      {
        // BackgroundColor = Color.Transparent;
        // BorderColor = Color.Transparent;
        OnDClick += (e) => SetChildrenOffset(new CUI3DOffset(0, 0, 1));

#if DEBUG
        this["wrapper"] = new CUIHorizontalList()
        {
          Anchor = new Vector2(1, 0),
          Fixed = true,
          Unserializable = true,
          FitContent = new CUIBool2(true, true),
        };

        this["wrapper"]["add"] = new CUIButton("add")
        {
          AddOnMouseDown = (e) => AddStuff(),
        };

        this["wrapper"]["save"] = new CUIButton("Save")
        {
          AddOnMouseDown = (e) => SaveToFile(Mod.ModDir + "/Ignore/test.xml"),
        };
        this["wrapper"]["load"] = new CUIButton("Load")
        {
          AddOnMouseDown = (e) => Showperf.LoadMap(),
        };
#endif
      }
    }
  }
}