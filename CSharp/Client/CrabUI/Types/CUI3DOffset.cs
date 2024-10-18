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
    public static float BaseZ = 1f;

    public float X;
    public float Y;
    public float Z;
    public float Scale => BaseZ + Z;

    public CUI3DOffset Shift(float x = 0, float y = 0)
    {
      return new CUI3DOffset(
        X + x * Scale,
        Y + y * Scale,
        Z
      );
    }

    public Vector2 ToPlaneCoords(Vector2 v) => ToPlaneCoords(v.X, v.Y);
    public Vector2 ToPlaneCoords(float x, float y) => new Vector2(x * Scale, y * Scale);

    public CUI3DOffset Zoom(Vector2 origin, float dZ) => Zoom(origin.X, origin.Y, dZ);
    public CUI3DOffset Zoom(float x, float y, float dZ)
    {
      return new CUI3DOffset(
        X,
        Y,
        Z + dZ
      );
    }
    public CUIRect Transform(CUIRect rect)
    {
      return new CUIRect(
        (rect.Left + X) / Scale,
        (rect.Top + Y) / Scale,
        rect.Width / Scale,
        rect.Height / Scale
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