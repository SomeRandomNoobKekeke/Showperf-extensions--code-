using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace CrabUI
{
  public struct CUI3DOffset
  {
    public float X;
    public float Y;
    public float Z;
    public float Scale => Z;
    public CUI3DOffset Shift(float x = 0, float y = 0, float z = 0) => new CUI3DOffset(X + x, Y + y, Z + z);
    public CUIRect Transform(CUIRect rect)
    {
      return new CUIRect(
        (rect.Left + X) / (1 + Z),
        (rect.Top + Y) / (1 + Z),
        rect.Width / (1 + Z),
        rect.Height / (1 + Z)
      );
    }

    public CUI3DOffset(float x = 0, float y = 0, float z = 0)
    {
      X = x;
      Y = y;
      Z = z;
    }

    public override string ToString() => $"[{X},{Y},{Z}]";
  }
}