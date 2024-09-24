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
  //TODO mb i should pack all props in one state and apply at once
  public static class CUIColors
  {
    public static Color ComponentBackground = Color.Black * 0.5f;
    public static Color ComponentBorder = Color.White * 0.5f;

    public static Color ButtonInactive = new Color(0, 0, 32);
    public static Color ButtonHover = new Color(0, 0, 64);
    public static Color ButtonPressed = new Color(0, 32, 127);


    public static Color ToggleButtonOff = new Color(0, 0, 32);
    public static Color ToggleButtonOn = new Color(0, 32, 127);
  }
}