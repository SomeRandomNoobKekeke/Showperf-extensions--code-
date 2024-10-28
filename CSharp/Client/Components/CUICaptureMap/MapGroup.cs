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
    public class MapGroup : CUIVerticalList
    {
      public CUITextBlock Header;
      public CUIVerticalList Content;

      public string Caption
      {
        get => Header.Text;
        set => Header.Text = value;
      }

      public CUIComponent Add(CUIComponent c) => Content.Append(c);

      public MapGroup() : base()
      {
        FitContent = new CUIBool2(true, true);
        ConsumeSwipe = true;
        Draggable = true;
        HideChildrenOutsideFrame = false;

        this["header"] = Header = new CUITextBlock("Header")
        {
          TextScale = 2.0f,
          Padding = new Vector2(0, 0),
        };

        this["content"] = Content = new CUIVerticalList()
        {
          FillEmptySpace = new CUIBool2(false, true),
          FitContent = new CUIBool2(true, true),
          HideChildrenOutsideFrame = false,
        };
      }
    }
  }
}