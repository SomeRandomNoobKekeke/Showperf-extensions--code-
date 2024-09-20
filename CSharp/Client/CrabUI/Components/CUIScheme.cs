using System;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace CrabUI
{
  public class CUIScheme : CUIComponent
  {
    public class CUISchemeLink
    {
      public CUIComponent Start;
      public CUIComponent End;

      public CUISchemeLink(CUIComponent start, CUIComponent end)
      {
        Start = start;
        End = end;
      }
    }

    public List<CUISchemeLink> Connections = new List<CUISchemeLink>();

    public void Connect(CUIComponent start, CUIComponent end)
    {
      Connections.Add(new CUISchemeLink(start, end));
    }
    protected override CUIRect DragZone => new CUIRect(-1000000, -1000000, 2000000, 2000000);

    public Color LineColor = Color.White;
    public float LineWidth = 2f;

    protected override void Draw(SpriteBatch spriteBatch)
    {
      base.Draw(spriteBatch);

      foreach (CUISchemeLink link in Connections)
      {
        GUI.DrawLine(spriteBatch, link.Start.Real.Center, link.End.Real.Center, LineColor, width: LineWidth);
      }
    }

    public CUIScheme() : base()
    {
    }

    public CUIScheme(float? x, float? y, float? w, float? h) : this()
    {
      Relative.Set(x, y, w, h);
    }
  }
}