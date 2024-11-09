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

using System.Xml;
using System.Xml.Linq;

namespace ShowPerfExtensions
{
  public partial class Plugin : IAssemblyPlugin
  {
    public partial class CUICaptureMap : CUIMap
    {
      public CUI3DOffset MemorizedOffset;

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
          c.ConsumeSwipe = true; //!locked;
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

      public override void FromXML(XElement element)
      {
        base.FromXML(element);
        //TODO all this "restore offset on dclick" stuff probably should be in CUIMap
        MemorizedOffset = this.ChildrenOffset;
      }


      public CUICaptureMap()
      {
        // BackgroundColor = Color.Transparent;
        // BorderColor = Color.Transparent;
        OnDClick += (e) => SetChildrenOffset(MemorizedOffset);

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
          AddOnMouseDown = (e) =>
          {
            SaveToFile(Mod.ModDir + "/XML/CUICaptureMap.xml");
            MemorizedOffset = ChildrenOffset;
          },
          InactiveColor = CUIPallete.Default.Tertiary.Off,
          MouseOverColor = CUIPallete.Default.Tertiary.OffHover,
          MousePressedColor = CUIPallete.Default.Tertiary.On,
          BorderColor = CUIPallete.Default.Tertiary.Border,
          DisabledColor = CUIPallete.Default.Tertiary.Disabled,

        };

        LoadButton = new CUIButton("Load")
        {
          AddOnMouseDown = (e) => Showperf.Pages.Open(Showperf.LoadMap()),
          InactiveColor = CUIPallete.Default.Tertiary.Off,
          MouseOverColor = CUIPallete.Default.Tertiary.OffHover,
          MousePressedColor = CUIPallete.Default.Tertiary.On,
          BorderColor = CUIPallete.Default.Tertiary.Border,
          DisabledColor = CUIPallete.Default.Tertiary.Disabled,
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


        Locked = true;
        OnChildAdded += (c) => PassLocked(c);
      }
    }
  }
}