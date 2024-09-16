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
  public struct CUIRect
  {
    public float Left;
    public float Top;
    public float Width;
    public float Height;

    public float Right => Left + Width;
    public float Bottom => Top + Height;

    public Vector2 Size => new Vector2(Width, Height);
    public Vector2 Position => new Vector2(Left, Top);
    public Vector2 RightBottom => new Vector2(Right, Bottom);

    public Rectangle Box => new Rectangle((int)Left, (int)Top, (int)Width, (int)Height);

    public bool Contains(float x, float y)
    {
      return Left < x && x < Right && Top < y && y < Bottom;
    }

    public bool Contains(Vector2 pos)
    {
      return Left < pos.X && pos.X < Right && Top < pos.Y && pos.Y < Bottom;
    }

    public CUIRect(float x, float y, float w, float h)
    {
      Left = x;
      Top = y;
      Width = Math.Max(0f, w);
      Height = Math.Max(0f, h);
    }

    public override string ToString() => $"[{Left}, {Top}, {Width}, {Height}]";
  }
  public class CUINullRect
  {
    public float? left; public float? Left
    {
      get => left;
      set { left = value; Host?.OnPropChanged(); }
    }
    public float? top; public float? Top
    {
      get => top;
      set { top = value; Host?.OnPropChanged(); }
    }

    public float? width; public float? Width
    {
      get => width;
      set { width = value.HasValue ? Math.Max(0f, value.Value) : value; Host?.OnPropChanged(); }
    }
    public float? height; public float? Height
    {
      get => height;
      set { height = value.HasValue ? Math.Max(0f, value.Value) : value; Host?.OnPropChanged(); }
    }

    public CUIComponent Host { get; set; }

    public Vector2 Size
    {
      get => new Vector2(Width ?? 0, Height ?? 0);
      set { Width = value.X; Height = value.Y; Host?.OnPropChanged(); }
    }
    public Vector2 Position
    {
      get => new Vector2(Left ?? 0, Top ?? 0);
      set { Left = value.X; Top = value.Y; Host?.OnPropChanged(); }
    }

    public CUINullRect(CUIComponent host = null)
    {
      Host = host;
    }

    public CUINullRect(float? x, float? y, float? w, float? h, CUIComponent host = null)
    {
      Left = x;
      Top = y;
      Width = w;
      Height = h;

      Host = host;
    }

    public override string ToString() => $"[{Left}, {Top}, {Width}, {Height}]";

  }
}