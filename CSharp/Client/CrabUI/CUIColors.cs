using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace CrabUI
{
  //TODO mb i should pack all props in one state and apply at once like a style
  //TODO mb this should be in some xml
  public static class CUIColors
  {
    public static Color ComponentBackground = Color.Black * 0.5f;
    public static Color ComponentBorder = Color.White * 0.5f;

    public static Color ButtonInactive = new Color(0, 0, 32);
    public static Color ButtonHover = new Color(0, 0, 64);
    public static Color ButtonPressed = new Color(0, 32, 128);

    public static Color ToggleButtonOff = new Color(0, 0, 32);
    public static Color ToggleButtonOn = new Color(0, 128, 64);


    public static Color DropDownBox = new Color(0, 0, 40);
    public static Color DropDownOption = Color.Transparent;
    public static Color DropDownOptionHover = new Color(0, 255, 255, 128);
  }
}