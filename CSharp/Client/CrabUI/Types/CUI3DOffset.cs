using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;
namespace CrabUI
{
  public struct CUI3DOffset
  {
    public float X;
    public float Y;
    public float Z;

    public Vector2 ToVector2 => new Vector2(X, Y);

    //TODO unhardcode and move to CUIBoundaries
    public static float MinZ = 1f;

    public CUI3DOffset Shift(Vector2 shift) => Shift(shift.X, shift.Y);
    public CUI3DOffset Shift(float x = 0, float y = 0)
    {
      return new CUI3DOffset(
        X + x * Z,
        Y + y * Z,
        Z
      );
    }

    public CUI3DOffset Zoom(Vector2 staticPoint, float dZ) => Zoom(staticPoint.X, staticPoint.Y, dZ);
    public CUI3DOffset Zoom(float sx, float sy, float dZ)
    {
      float newZ = Math.Max(MinZ, Z + dZ);
      Vector2 s1 = new Vector2(sx * Z - X, sy * Z - Y);
      Vector2 s2 = new Vector2(sx * newZ - X, sy * newZ - Y);
      Vector2 d = s2 - s1;

      return new CUI3DOffset(X + d.X, Y + d.Y, newZ);
    }

    public Vector2 ToPlaneCoords(Vector2 v)
    {
      return new Vector2(v.X * Z - X, v.Y * Z - Y);
    }
    public CUIRect Transform(CUIRect rect)
    {
      return new CUIRect(
        (rect.Left + X) / Z,
        (rect.Top + Y) / Z,
        rect.Width / Z,
        rect.Height / Z
      );
    }

    // XGLBRLGRLXBRSLRLGRKK!!!
    public CUI3DOffset()
    {
      X = 0;
      Y = 0;
      Z = 1;
    }

    public CUI3DOffset(float x, float y, float z)
    {
      X = x;
      Y = y;
      Z = z;
    }

    public override string ToString() => $"[{X},{Y},{Z}]";
  }
}