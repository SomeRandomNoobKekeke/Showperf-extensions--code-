using System;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace CrabUI
{
  public class CUIHorizontalList : CUIComponent
  {

    public CUIHorizontalList(float x, float y, float w, float h) : base(x, y, w, h)
    {
      HideChildrenOutsideFrame = true;
      Layout = new CUILayoutList(this, vertical: false);
    }
  }
}