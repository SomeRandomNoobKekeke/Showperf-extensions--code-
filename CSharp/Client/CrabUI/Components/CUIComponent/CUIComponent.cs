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
    #region Static --------------------------------------------------------
    public static int MaxID = 0;
    public static Dictionary<int, CUIComponent> ComponentsById = new Dictionary<int, CUIComponent>();
    public static Vector2 GameScreenSize => new Vector2(GameMain.GraphicsWidth, GameMain.GraphicsHeight);
    public static Rectangle GameScreenRect => new Rectangle(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight);
    public static GUIButton dummyComponent = new GUIButton(new RectTransform(new Point(0, 0)));
    public static void RunRecursiveOn(CUIComponent component, Action<CUIComponent, int> action, int depth = 0)
    {
      action(component, depth);
      foreach (CUIComponent child in component.Children)
      {
        RunRecursiveOn(child, action, depth + 1);
      }
    }




    #endregion
    #region Virtual --------------------------------------------------------

    internal virtual CUIBoundaries ChildrenBoundaries => new CUIBoundaries();
    internal virtual CUIBoundaries ChildOffsetBounds => new CUIBoundaries();
    internal virtual void UpdatePseudoChildren()
    {
      LeftResizeHandle.Update();
      RightResizeHandle.Update();
    }
    internal virtual Vector2 AmIOkWithThisSize(Vector2 size) => size;

    //Never used + cursed
    //internal virtual void ChildrenSizeCalculated() { }
    public virtual partial void ApplyState(CUIComponent state);
    public virtual partial void Draw(SpriteBatch spriteBatch);
    public virtual partial void DrawFront(SpriteBatch spriteBatch);

    #endregion
    #region Meta --------------------------------------------------------

    public int ID;
    public bool DebugHighlight;


    //TODO should CulledOut be propagated to children?
    internal bool CulledOut;
    //HACK need a more robust solution
    protected bool ComponentInitialized;

    public override string ToString() => $"{this.GetType().Name}:{ID}:{AKA}";

    public CUIRect BorderBox;
    public Rectangle? ScissorRect;
    private CUIRect real; public CUIRect Real
    {
      get => real;
      set => SetReal(value);
    }

    internal void SetReal(CUIRect value, [CallerMemberName] string memberName = "")
    {
      real = value;
      CUIDebug.Capture(null, this, "SetReal", memberName, "real", real.ToString());

      BorderBox = new CUIRect(
        real.Left - BorderThickness,
        real.Top - BorderThickness,
        real.Width + BorderThickness * 2,
        real.Height + BorderThickness * 2
      );

      if (HideChildrenOutsideFrame)
      {
        //HACK Remove these + 1
        Rectangle SRect = new Rectangle(
          (int)real.Left + 1,
          (int)real.Top + 1,
          (int)real.Width - 2,
          (int)real.Height - 2
        );

        if (Parent?.ScissorRect != null)
        {
          ScissorRect = Rectangle.Intersect(Parent.ScissorRect.Value, SRect);
        }
        else
        {
          ScissorRect = SRect;
        }
      }
      else ScissorRect = Parent?.ScissorRect;
    }

    #endregion
    #region Draw --------------------------------------------------------

    public virtual partial void Draw(SpriteBatch spriteBatch)
    {
      if (BackgroundVisible) GUI.DrawRectangle(spriteBatch, Real.Position, Real.Size, BackgroundColor, isFilled: true);

      if (BorderVisible) GUI.DrawRectangle(spriteBatch, BorderBox.Position, BorderBox.Size, BorderColor, thickness: BorderThickness);

      LeftResizeHandle.Draw(spriteBatch);
      RightResizeHandle.Draw(spriteBatch);
    }

    public virtual partial void DrawFront(SpriteBatch spriteBatch)
    {
      if (DebugHighlight)
      {
        GUI.DrawRectangle(spriteBatch, Real.Position, Real.Size, Color.Cyan * 0.5f, isFilled: true);
      }
    }


    #endregion
    #region Constructors --------------------------------------------------------
    public CUIComponent()
    {
      ID = MaxID++;
      ComponentsById[ID] = this;

      BackgroundColor = CUIPallete.Default.Primary.Off;
      BorderColor = CUIPallete.Default.Primary.Border;

      Layout = new CUILayoutSimple();

      DragHandle = new CUIDragHandle(this);
      SwipeHandle = new CUISwipeHandle(this);
      LeftResizeHandle = new CUIResizeHandle(this, anchor: new Vector2(0, 1));
      RightResizeHandle = new CUIResizeHandle(this, anchor: new Vector2(1, 1));

      ComponentInitialized = true;
    }

    public CUIComponent(float? x = null, float? y = null, float? w = null, float? h = null) : this()
    {
      Relative = new CUINullRect(x, y, w, h);
    }
    #endregion
  }
}