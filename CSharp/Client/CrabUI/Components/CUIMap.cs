using System;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace CrabUI
{
  public class CUIMapContent : CUIComponent
  {
    private class CUIMapLink
    {
      public CUIComponent Start;
      public CUIComponent End;
      public float LineWidth;
      public Color LineColor;

      public CUIMapLink(CUIComponent start, CUIComponent end, Color? lineColor = null, float lineWidth = 2f)
      {
        LineColor = lineColor ?? Color.White;
        LineWidth = lineWidth;
        Start = start;
        End = end;
      }
    }

    private List<CUIMapLink> Connections = new List<CUIMapLink>();

    internal override CUINullRect ChildrenBoundaries => new CUINullRect(null, null, null, null);

    public void Connect(CUIComponent start, CUIComponent end, Color? color = null)
    {
      Connections.Add(new CUIMapLink(start, end, color));
    }

    protected override void Draw(SpriteBatch spriteBatch)
    {
      base.Draw(spriteBatch);

      foreach (CUIMapLink link in Connections)
      {
        GUI.DrawLine(spriteBatch, link.Start.Real.Center, link.End.Real.Center, link.LineColor, width: link.LineWidth);
      }
    }

    public CUIMapContent() : base()
    {
      BackgroundColor = Color.Transparent;
      BorderColor = Color.Transparent;
      UnCullable = true;
    }

    public CUIMapContent(float? x = null, float? y = null, float? w = null, float? h = null) : this()
    {
      Relative = new CUINullRect(x, y, w, h);
    }
  }

  public class CUIMap : CUIComponent
  {
    public CUIMapContent Map;
    public CUIComponent Add(CUIComponent c) => Map.Append(c);
    public CUIComponent Add(string name, CUIComponent c)
    {
      if (name != null) Remember(c, name);
      return Map.Append(c);
    }

    public void Connect(CUIComponent start, CUIComponent end, Color? color = null)
    {
      Map.Connect(start, end, color);
    }
    public void Connect(int start, int end, Color? color = null)
    {
      CUIComponent startComponent = Map.Children.ElementAtOrDefault(start);
      CUIComponent endComponent = Map.Children.ElementAtOrDefault(end);
      if (startComponent != null && endComponent != null)
      {
        Connect(startComponent, endComponent, color);
      }
    }

    public CUIMap() : base()
    {
      Swipeable = true;
      ConsumeMouseClicks = true;
      HideChildrenOutsideFrame = true;

      this.Append(Map = new CUIMapContent());
    }

    public CUIMap(float? x = null, float? y = null, float? w = null, float? h = null) : this()
    {
      Relative = new CUINullRect(x, y, w, h);
    }
  }

}