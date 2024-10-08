using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CrabUI
{
  public static partial class CUITest
  {
    public static void CUIDropDown(CUIMainComponent CUI)
    {
      CUIComponent f = new CUIFrame(0.6f, 0.2f, 0.4f, 0.6f)
      {
        BackgroundColor = Color.Green,
      };

      CUIDropDown dd = new CUIDropDown(0.2f, 0.2f, null, null);
      dd.Add("1");
      dd.Add("22");
      CUIDropDown.Option o1 = (CUIDropDown.Option)dd.Add("333");
      CUIDropDown.Option o2 = (CUIDropDown.Option)dd.Add("4444");
      dd.Add("55555");
      dd.Add("666666");

      dd.Select("1");

      f.Append(dd);
      f.Debug = true;
      CUI.Append(f);

      CUIComponent controls = new CUIFrame(0.4f, 0.2f, 0.2f, 0.4f)
      {
        BackgroundColor = Color.Green,
      };

      controls["list"] = new CUIVerticalList(0, 0, 1, 1);
      controls["list"]["b1"] = new CUIButton("change text");
      controls["list"]["b1"].OnMouseDown += (m) => o1.Text = o1.Text == "333" ? "23409123409182394" : "333";
      controls["list"]["b2"] = new CUIButton("change text");
      controls["list"]["b2"].OnMouseDown += (m) => o2.Text = o2.Text == "4444" ? "23409123409182394" : "4444";

      CUI.Append(controls);
    }
  }
}