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
  public partial class CUIComponent
  {
    public static int MaxID = 0;
    public int ID;

    public virtual string Name => "CUIComponent";

    public static Vector2 GameScreenSize => new Vector2(GameMain.GraphicsWidth, GameMain.GraphicsHeight);
    public static GUIButton dummyComponent = new GUIButton(new RectTransform(new Point(0, 0)));

    public List<CUIComponent> Children = new List<CUIComponent>();
    public CUIComponent? parent; public CUIComponent? Parent
    {
      get => parent;
      set { parent = value; TreeChanged = true; }
    }

    public bool treeChanged = true; public bool TreeChanged
    {
      get => treeChanged;
      set { treeChanged = value; if (value && Parent != null) Parent.TreeChanged = true; }
    }

    public void AddToUpdateQueueRecursive(List<CUIComponent> queue)
    {
      queue.Add(this);
      TreeChanged = false;
      Children.ForEach(c => c.AddToUpdateQueueRecursive(queue));
    }




    public Color BackgroundColor = Color.Black * 0.5f;
    public Color BorderColor = Color.White * 0.5f;
    public float BorderThickness = 1f;


    public bool Visible { get; set; } = true;
    public bool HideChildrenOutsideFrame { get; set; } = false;


    public CUIComponent Append(CUIComponent c)
    {
      Children.Add(c);
      c.Parent = this;
      ApplyParentSizeRestrictions(c);
      NeedsLayoutUpdate = true;

      log($"{this} <- {c}");

      return c;
    }
    public virtual void Update() { }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
      GUI.DrawRectangle(spriteBatch, RealPosition, Size, BackgroundColor, isFilled: true);
      GUI.DrawRectangle(spriteBatch, RealPosition, Size, BorderColor);
    }
    public static void UpdateRecursive(CUIComponent component)
    {
      component.Update();
      component.Children.ForEach(c => c.Update());
    }

    public static void DrawRecursive(SpriteBatch spriteBatch, CUIComponent component)
    {
      if (component.Visible) component.Draw(spriteBatch);

      Rectangle prevScissorRect = spriteBatch.GraphicsDevice.ScissorRectangle;

      if (component.HideChildrenOutsideFrame)
      {
        spriteBatch.End();
        spriteBatch.GraphicsDevice.ScissorRectangle = Rectangle.Intersect(prevScissorRect, component.BorderBox);
        spriteBatch.Begin(SpriteSortMode.Deferred, samplerState: GUI.SamplerState, rasterizerState: GameMain.ScissorTestEnable);
      }

      component.Children.ForEach(c => DrawRecursive(spriteBatch, c));

      if (component.HideChildrenOutsideFrame)
      {
        spriteBatch.End();
        spriteBatch.GraphicsDevice.ScissorRectangle = prevScissorRect;
        spriteBatch.Begin(SpriteSortMode.Deferred, samplerState: GUI.SamplerState, rasterizerState: GameMain.ScissorTestEnable);
      }
    }





    public CUIComponent()
    {
      ID = MaxID++;
    }

    public CUIComponent(float x, float y, float w, float h) : this()
    {
      RelativeLeft = x;
      RelativeTop = y;
      RelativeWidth = w;
      RelativeHeight = h;
    }

    public CUIComponent(Vector2 relativePosition, Vector2 relativeSize)
    {
      RelativePosition = relativePosition;
      RelativeSize = relativeSize;
    }

    public override string ToString()
    {
      return $"{ID}:{Name}";
    }

    public static void log(object msg, Color? cl = null)
    {
      cl ??= Color.Cyan;
      LuaCsLogger.LogMessage($"{msg ?? "null"}", cl * 0.8f, cl);
    }
  }
}