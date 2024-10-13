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
    #region Layout --------------------------------------------------------

    private CUILayout layout; public CUILayout Layout
    {
      get => layout;
      set { layout = value; layout.Host = this; }
    }

    private Vector2 childrenOffset; public Vector2 ChildrenOffset
    {
      get => childrenOffset;
      set => SetChildrenOffset(value);
    }
    internal void SetChildrenOffset(Vector2 value, [CallerMemberName] string memberName = "")
    {
      childrenOffset = value;
      // if (ComponentInitialized)
      // {
      //   CUIDebug.Capture(null, this, "SetChildrenOffset", memberName, "childrenOffset", childrenOffset.ToString());
      // }
      OnChildrenPropChanged();
    }

    internal void OnPropChanged([CallerMemberName] string memberName = "")
    {
      Layout.Changed = true;
      if (ComponentInitialized)
      {
        CUIDebug.Capture(null, this, "OnPropChanged", memberName, "Layout.Changed", "true");
      }
    }
    internal void OnDecorPropChanged([CallerMemberName] string memberName = "")
    {
      Layout.DecorChanged = true;
      if (ComponentInitialized)
      {
        CUIDebug.Capture(null, this, "OnDecorPropChanged", memberName, "Layout.DecorChanged", "true");
      }
    }
    internal void OnAbsolutePropChanged([CallerMemberName] string memberName = "")
    {
      Layout.AbsoluteChanged = true;
      if (ComponentInitialized)
      {
        CUIDebug.Capture(null, this, "OnAbsolutePropChanged", memberName, "Layout.AbsoluteChanged", "true");
      }
    }
    internal void OnChildrenPropChanged([CallerMemberName] string memberName = "")
    {
      foreach (CUIComponent child in Children)
      {
        child.Layout.Changed = true;
      }
    }

    #endregion

  }
}