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
  // I'm not an expert in color theory, but at least some order
  public class CUIColorPreset
  {
    public Color Border;
    public Color Text;
    public Color Off;
    public Color OffHover;
    public Color On;
    public Color OnHover;
    public Color Disabled;
  }

  public class CUIPallete
  {
    public static CUIPallete DarkBlue = new CUIPallete()
    {
      Primary = new CUIColorPreset()
      {
        Border = new Color(255, 255, 255, 128),
        Text = new Color(255, 255, 255, 255),
        Off = new Color(0, 0, 0, 128),
      },
      Secondary = new CUIColorPreset()
      {
        Border = new Color(255, 255, 255, 128),
        Text = new Color(255, 255, 255, 255),
        Off = new Color(0, 0, 32, 255),
        OffHover = new Color(0, 0, 64, 255),
        On = new Color(0, 0, 255, 255),
        OnHover = new Color(0, 0, 196, 255),
      },
      Tertiary = new CUIColorPreset()
      {
        Border = new Color(255, 255, 255, 128),
        Text = new Color(255, 255, 255, 255),
        Off = new Color(32, 0, 32, 255),
        OffHover = new Color(64, 0, 64, 255),
      },
    };

    public static CUIPallete Default => DarkBlue;

    public CUIColorPreset Primary = new CUIColorPreset();
    public CUIColorPreset Secondary = new CUIColorPreset();
    public CUIColorPreset Tertiary = new CUIColorPreset();
  }
}