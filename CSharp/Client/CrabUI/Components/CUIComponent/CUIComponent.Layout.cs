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

    private CUI3DOffset childrenOffset = new CUI3DOffset(); public CUI3DOffset ChildrenOffset
    {
      get => childrenOffset;
      set => SetChildrenOffset(value);
    }
    internal void SetChildrenOffset(CUI3DOffset value, [CallerMemberName] string memberName = "")
    {
      childrenOffset = ChildOffsetBounds.Check(value);
      CUIDebug.Capture(null, this, "SetChildrenOffset", memberName, "ChildrenOffset", ChildrenOffset.ToString());
      foreach (var child in Children)
      {
        if (!child.Fixed) child.Scale = value.Z;
      }
      OnChildrenPropChanged();
    }

    internal void OnPropChanged([CallerMemberName] string memberName = "")
    {
      Layout.Changed = true;
      CUIDebug.Capture(null, this, "OnPropChanged", memberName, "Layout.Changed", "true");
    }
    internal void OnDecorPropChanged([CallerMemberName] string memberName = "")
    {
      Layout.DecorChanged = true;
      CUIDebug.Capture(null, this, "OnDecorPropChanged", memberName, "Layout.DecorChanged", "true");
    }
    internal void OnAbsolutePropChanged([CallerMemberName] string memberName = "")
    {
      Layout.AbsoluteChanged = true;
      CUIDebug.Capture(null, this, "OnAbsolutePropChanged", memberName, "Layout.AbsoluteChanged", "true");
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