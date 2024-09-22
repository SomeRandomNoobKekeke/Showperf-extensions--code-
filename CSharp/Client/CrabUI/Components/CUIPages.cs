using System;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace CrabUI
{
  public class CUIPages : CUIComponent
  {
    public CUIComponent OpenedPage;

    public bool IsOpened(CUIComponent p) => OpenedPage == p;

    public void Open(CUIComponent p)
    {
      RemoveAllChildren();
      Append(p);
      OpenedPage = p;
    }

    public CUIPages() : base()
    {
      BackgroundColor = Color.Transparent;
      BorderColor = Color.Transparent;
      HideChildrenOutsideFrame = true;
    }

    public CUIPages(float? x, float? y, float? w, float? h) : this()
    {
      Relative.Set(x, y, w, h);
    }
  }
}