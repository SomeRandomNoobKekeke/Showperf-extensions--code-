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



    public CUIScheme() : base()
    {
    }

    public CUIScheme(float? x, float? y, float? w, float? h) : this()
    {
      Relative.Set(x, y, w, h);
    }
  }
}