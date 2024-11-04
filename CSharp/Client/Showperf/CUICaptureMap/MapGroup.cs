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

using System.Xml;
using System.Xml.Linq;

namespace ShowPerfExtensions
{
  public partial class Plugin : IAssemblyPlugin
  {
    public class MapGroup : CUIVerticalList
    {
      [DontSerializeAttribute]
      public new CUINullRect AbsoluteMin { get => absoluteMin; set => SetAbsoluteMin(value); }
      public CUITextBlock Header;
      public CUIVerticalList Content;

      public string Caption
      {
        get => Header.Text;
        set => Header.Text = value;
      }

      public CUIComponent Add(CUIComponent c) => Content.Append(c);


      public override void FromXML(XElement element)
      {
        this.RemoveAllChildren();
        Header = null;
        Content = null;
        base.FromXML(element);

        Header = (CUITextBlock)Children.ElementAtOrDefault(0);
        Content = (CUIVerticalList)Children.ElementAtOrDefault(1);
      }

      protected override CUIComponent RawGet(string name) => Content.RawGet(name);
      public MapGroup() : base()
      {
        FitContent = new CUIBool2(true, true);
        ConsumeSwipe = false;
        ConsumeDragAndDrop = true;
        HideChildrenOutsideFrame = false;
        BackgroundColor = Color.Black;

        Header = new CUITextBlock("Header")
        {
          TextScale = 1.8f,
        };

        Content = new CUIVerticalList()
        {
          //FillEmptySpace = new CUIBool2(false, true),
          FitContent = new CUIBool2(true, true),
          Debug = true,
        };

        this.Append(Header);
        this.Append(Content);
      }
    }
  }
}