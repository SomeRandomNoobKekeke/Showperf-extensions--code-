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
    public event Action<CUIButton, int> OnSelect;
    public Action<CUIButton, int> AddOnSelect { set { OnSelect += value; } }

    //TODO this could store any component and not just buttons
    public List<CUIButton> Buttons = new List<CUIButton>();

    public CUIButton Selected;

    public CUIButton Add(CUIButton b)
    {
      Buttons.Add(b);
      b.OnMouseDown += (m) => SelectNext(b);
      b.Relative = new CUINullRect(0, 0, 1f, 1f);
      b.MousePressedColor = b.MouseOverColor; // HACK remove this kostyl
      //b.MouseOverColor = b.MousePressedColor; //HACK remove this kostyl
      b.ConsumeDragAndDrop = ConsumeDragAndDrop;

      return b;
    }

    public void SelectNext(CUIButton b)
    {
      int i = Buttons.IndexOf(b);
      if (i != -1) Select(i + 1);
    }


    public void Select(object data, bool silent = false)
    {
      //TODO investigate why simple == doesnt work
      // a ok, because there's some boxing going on
      //TODO and what would happen if data is another CUIButton
      CUIButton btn = Buttons.Find(b => b.Data.GetHashCode() == data.GetHashCode());
      if (btn != null) Select(btn, silent);
    }
    public void Select(CUIButton b, bool silent = false)
    {
      int i = Buttons.IndexOf(b);
      if (i != -1) Select(i, silent);
    }

    public void Select(int i, bool silent = false)
    {
      if (Buttons.Count == 0) return;

      RemoveAllChildren();
      int realIndex = i % Buttons.Count;
      Selected = Buttons[realIndex];
      Append(Selected);

      if (!silent) OnSelect?.Invoke(Selected, realIndex);
    }

    // internal override Vector2 AmIOkWithThisSize(Vector2 size)
    // {
    //   if (Selected == null) return size;
    //   return Selected.AmIOkWithThisSize(size);
    // }


    public CUIMultiButton() : base()
    {
      FitContent = new CUIBool2(true, true);
      ConsumeDragAndDrop = true;
    }
  }
}