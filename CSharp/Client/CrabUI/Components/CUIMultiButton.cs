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
  public class CUIMultiButton : CUIComponent
  {
    public event Action<CUIButton> OnSelect;

    //TODO this could store any component and no just buttons
    public List<CUIButton> Buttons = new List<CUIButton>();

    public CUIButton Selected;

    public CUIButton Add(CUIButton b)
    {
      Buttons.Add(b);
      b.OnMouseDown += (m) => SelectNext(b);
      b.Relative = new CUINullRect(0, 0, 1, 1);
      b.MousePressedColor = b.MouseOverColor; //HACK remove this kostyl
      return b;
    }

    public void SelectNext(CUIButton b)
    {
      int i = Buttons.IndexOf(b);
      if (i != -1) Select(i + 1);
    }

    public void Select(CUIButton b)
    {
      int i = Buttons.IndexOf(b);
      if (i != -1) Select(i);
    }

    public void Select(int i)
    {
      if (Buttons.Count == 0) return;

      RemoveAllChildren();
      Selected = Buttons[i % Buttons.Count];
      Append(Selected);
      OnSelect?.Invoke(Selected);
    }

    internal override Vector2 AmIOkWithThisSize(Vector2 size)
    {
      if (Selected == null) return size;
      return Selected.AmIOkWithThisSize(size);
    }


    public CUIMultiButton() : base()
    {
      FitContent = new CUIBool2(true, true);
    }

    public CUIMultiButton(float? width, float? height) : this(null, null, width, height) { }

    public CUIMultiButton(float? x, float? y, float? w, float? h) : this()
    {
      Relative = new CUINullRect(x, y, w, h);
    }
  }
}