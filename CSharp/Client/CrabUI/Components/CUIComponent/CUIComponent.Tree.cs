using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.IO;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using System.Xml;
using System.Xml.Linq;

namespace CrabUI
{
  public partial class CUIComponent
  {
    #region Tree --------------------------------------------------------

    public List<CUIComponent> Children = new List<CUIComponent>();

    private CUIComponent? parent; public CUIComponent? Parent
    {
      get => parent;
      set => SetParent(value);
    }
    internal void SetParent(CUIComponent? value, [CallerMemberName] string memberName = "")
    {
      if (parent != null)
      {
        OnPropChanged();
        TreeChanged = true;
      }
      parent = value;
      if (ComponentInitialized)
      {
        CUIDebug.Capture(null, this, "SetParent", memberName, "parent", $"{parent}");
      }
      if (parent != null)
      {
        OnPropChanged();
        TreeChanged = true;
      }
    }


    private bool treeChanged = true; public bool TreeChanged
    {
      get => treeChanged;
      set { treeChanged = value; if (value && Parent != null) Parent.TreeChanged = true; }
    }

    public virtual CUIComponent Append(CUIComponent c, string name = null)
    {
      if (c == null) return c;

      Children.Add(c);
      c.SetParent(this);

      PassPropsToChild(c);

      if (name != null)
      {
        NamedComponents[name] = c;
        c.AKA = name;
      }

      return c;
    }

    public virtual CUIComponent Prepend(CUIComponent c, string name = null)
    {
      if (c == null) return c;

      Children.Insert(0, c);
      c.SetParent(this);

      PassPropsToChild(c);

      if (name != null)
      {
        NamedComponents[name] = c;
        c.AKA = name;
      }

      return c;
    }

    public void RemoveSelf() => Parent?.RemoveChild(this);
    public void RemoveChild(CUIComponent c)
    {
      if (c == null || !Children.Contains(c)) return;

      if (c.AKA != null && NamedComponents.ContainsKey(c.AKA))
      {
        NamedComponents.Remove(c.AKA);
        //c.AKA = null;
      }
      c.SetParent(null);
      Children.Remove(c);
    }



    public void RemoveAllChildren()
    {
      foreach (CUIComponent c in Children) { c.SetParent(null); }
      NamedComponents.Clear();
      Children.Clear();
    }


    private void PassPropsToChild(CUIComponent child)
    {
      if (!ShouldPassPropsToChildren) return;

      if (ZIndex.HasValue) child.ZIndex = ZIndex.Value + 1;
      if (IgnoreEvents) child.IgnoreEvents = true;
      if (!Visible) child.Visible = false;
    }

    #endregion
  }
}