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
    public static CUIPages Default = new CUIPages();
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
      HideChildrenOutsideFrame = false;
    }
  }
}