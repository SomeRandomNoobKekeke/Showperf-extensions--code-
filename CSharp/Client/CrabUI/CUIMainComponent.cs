using System;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace CrabUI
{
  public class CUIMainComponent : CUIComponent
  {
    public virtual string Name => "CUIMainComponent";

    public static CUIMainComponent Main;
    public static void GUI_Draw_Prefix(SpriteBatch spriteBatch)
    {
      try
      {
        Main?.Step(spriteBatch);
      }
      catch (Exception e)
      {
        log(e);
      }
    }

    // public void AttachToGUICycle(){ }


    public List<CUIComponent> UpdateQueue = new List<CUIComponent>();

    public CUIComponent MouseOn;
    public void RebuildUpdateQueue()
    {
      UpdateQueue ??= new List<CUIComponent>();
      UpdateQueue.Clear();

      this.AddToUpdateQueueRecursive(UpdateQueue);
    }

    public void Step(SpriteBatch spriteBatch)
    {
      if (TreeChanged) RebuildUpdateQueue();

      for (int i = 0; i < UpdateQueue.Count; i++)
      {
        UpdateQueue[i].ApplyRelSize();
      }

      // in reverse
      for (int i = UpdateQueue.Count - 1; i >= 0; i--)
      {
        if (UpdateQueue[i].NeedsContentSizeUpdate)
        {
          Vector2 contentSize = UpdateQueue[i].CalculateContentSize();
          UpdateQueue[i].Size = new Vector2(
            Math.Max(UpdateQueue[i].Size.X, contentSize.X),
            Math.Max(UpdateQueue[i].Size.Y, contentSize.Y)
          );

          UpdateQueue[i].NeedsContentSizeUpdate = false;
        }
      }

      for (int i = 0; i < UpdateQueue.Count; i++)
      {
        if (UpdateQueue[i].NeedsLayoutUpdate)
        {
          UpdateQueue[i].UpdateLayout();
          UpdateQueue[i].NeedsLayoutUpdate = false;
        }
      }

      //in reverse
      for (int i = UpdateQueue.Count - 1; i >= 0; i--)
      {
        if (UpdateQueue[i].BorderBox.Contains(PlayerInput.MousePosition))
        {
          if (UpdateQueue[i] != MouseOn)
          {
            if (MouseOn != null)
            {
              MouseOn.OnMouseLeave?.Invoke();
              MouseOn.MouseOver = false;
            }

            UpdateQueue[i].OnMouseEnter?.Invoke();
            UpdateQueue[i].MouseOver = true;

            MouseOn = UpdateQueue[i];
          }

          UpdateQueue[i].MousePressed = PlayerInput.PrimaryMouseButtonHeld();

          if (PlayerInput.PrimaryMouseButtonDown())
          {
            UpdateQueue[i]?.OnMouseDown?.Invoke();
          }

          if (PlayerInput.PrimaryMouseButtonClicked())
          {
            UpdateQueue[i]?.OnMouseClick?.Invoke();
          }

          // GUI.MouseOn = dummyComponent;

          break;
        }
      }


      DrawRecursive(spriteBatch, this);
    }



    public CUIMainComponent()
    {
      Main = this;

      RelativeSize = Vector2.One;
      ApplyRelSize();
      Visible = false;
    }

    public void Dispose()
    {
      Main = null;
    }
  }
}