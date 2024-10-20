using System;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace CrabUI
{
  public class CUIMap : CUIComponent
  {
    public class CUIMapLink
    {
      public CUIComponent Start;
      public CUIComponent End;
      public float LineWidth;
      public Color LineColor;

      public CUIMapLink(CUIComponent start, CUIComponent end, Color? lineColor = null, float lineWidth = 2f)
      {
        LineColor = lineColor ?? new Color(128, 128, 128);
        LineWidth = lineWidth;
        Start = start;
        End = end;
      }
    }
    public class LinksContainer : CUIComponent
    {
      public List<CUIMapLink> Connections = new List<CUIMapLink>();

      public override void Draw(SpriteBatch spriteBatch)
      {
        base.Draw(spriteBatch);

        foreach (CUIMapLink link in Connections)
        {
          GUI.DrawLine(spriteBatch, link.Start.Real.Center, link.End.Real.Center, link.LineColor, width: link.LineWidth);
        }
      }

      public LinksContainer()
      {
        UnCullable = true;
        BackgroundColor = Color.Transparent;
        BorderColor = Color.Transparent;
      }
    }

    public LinksContainer linksContainer;



    public CUIComponent Add(CUIComponent c) => Append(c);
    public CUIComponent Add(string name, CUIComponent c)
    {
      if (name != null) Remember(c, name);
      return Append(c);
    }



    public CUIComponent Connect(CUIComponent startComponent, CUIComponent endComponent, Color? color = null)
    {
      if (startComponent != null && endComponent != null)
      {
        if (color == null && (!startComponent.Disabled || !endComponent.Disabled)) color = new Color(0, 0, 255);
        linksContainer.Connections.Add(new CUIMapLink(startComponent, endComponent, color));
      }
      return startComponent;
    }
    public CUIComponent Connect(CUIComponent startComponent, int end = -2, Color? color = null)
    {
      end = MathUtils.PositiveModulo(end, Children.Count);
      CUIComponent endComponent = Children.ElementAtOrDefault(end);
      return Connect(startComponent, endComponent, color);
    }
    public CUIComponent Connect(int start, int end, Color? color = null)
    {
      start = MathUtils.PositiveModulo(start, Children.Count);
      end = MathUtils.PositiveModulo(end, Children.Count);

      CUIComponent startComponent = Children.ElementAtOrDefault(start);
      CUIComponent endComponent = Children.ElementAtOrDefault(end);
      return Connect(startComponent, endComponent, color);
    }

    public CUIComponent ConnectTo(CUIComponent Host, params CUIComponent[] children)
    {
      foreach (CUIComponent child in children) { Connect(Host, child); }
      return Host;
    }




    public CUIMap() : base()
    {
      Swipeable = true;
      ConsumeMouseClicks = true;
      HideChildrenOutsideFrame = true;
      BackgroundColor = Color.Transparent;

      //without container links won't be culled
      this["links"] = linksContainer = new LinksContainer();

      //TODO the main todo of this branch
      OnScroll += (m) =>
      {
        SetChildrenOffset(
          ChildrenOffset.Zoom(
            m.MousePosition - Real.Position,
            (-m.Scroll / 500f)
          )
        );
      };
    }
  }

}