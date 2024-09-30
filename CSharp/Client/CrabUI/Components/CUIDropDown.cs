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
  public class CUIDropDown : CUIButton
  {
    public class CUIDropDownBox : CUIVerticalList
    {
      public CUIDropDown Host;

      public List<CUIDropDownOption> Options = new List<CUIDropDownOption>();

      private bool opened; public bool Opened
      {
        get => opened;
        set
        {
          opened = value;
          if (opened) { Visible = true; IgnoreEvents = false; }
          if (!opened) { Visible = false; IgnoreEvents = true; }
        }
      }


      public void Open() => Opened = true;
      public void Close() => Opened = false;
      public void Toggle() => Opened = !Opened;


      //HACK mmm... sneaky text objects
      public CUIDropDownOption Add(object text, object value = null)
      {
        value ??= text;

        CUIDropDownOption o = new CUIDropDownOption(text.ToString(), value.ToString(), Host);

        Options.Add(o);
        Append(o);

        return o;
      }

      public CUIDropDownBox(CUIDropDown host) : base()
      {
        Host = host;

        Visible = false;
        IgnoreEvents = true;

        Relative = new CUINullRect(0, 1, 1, null);
        FitContent = new CUIBool2(true, true);

        ConsumeMouseClicks = true;
        ConsumeDragAndDrop = true;
        ConsumeSwipe = true;
        HideChildrenOutsideFrame = false;
        ZIndex = 100;

        BackgroundColor = CUIPallete.Default.Tertiary.Off;
        BorderColor = CUIPallete.Default.Tertiary.Border;

        OnMouseDown += (m) => Close();

        CUI.Main.OnMouseDown += (m) => Close();

        Close();
      }
    }


    public class CUIDropDownOption : CUITextBlock
    {
      public CUIDropDown Host;
      //TODO mb Value should be object
      public string Value;
      public Color HoverColor;

      protected override void Draw(SpriteBatch spriteBatch)
      {
        BackgroundColor = Color.Transparent;
        if (MouseOver) BackgroundColor = HoverColor;
        base.Draw(spriteBatch);
      }

      public CUIDropDownOption(string text, string value, CUIDropDown host) : base(text)
      {
        Host = host;
        Value = value;

        HoverColor = CUIPallete.Default.Tertiary.OffHover;
        TextColor = CUIPallete.Default.Tertiary.Text;

        Relative = new CUINullRect(0, null, 1, null);

        OnMouseDown += (CUIMouse m) => Host.Select(this);
        OnMouseDown += (CUIMouse m) => SoundPlayer.PlayUISound(Host.ClickSound);
      }
    }
    public GUISoundType ClickSound { get; set; } = GUISoundType.Select;


    public CUIDropDownBox Box;
    public CUIDropDownOption Selected;

    public CUIDropDownOption Add(object text, object value = null) => Box.Add(text, value);
    public void Open() => Box.Open();
    public void Close() => Box.Close();
    public void Toggle() => Box.Toggle();

    public event Action<string> OnSelect;

    public void Select(object value) => Select(Box.Options.Find(o => o.Value == value.ToString()));
    public void Select(CUIDropDownOption option)
    {
      if (option == null) return;
      Selected = option;
      OnSelect?.Invoke(Selected.Value);
      Text = option.Text;
    }


    public CUIDropDown() : base("CUIDropDown")
    {
      FitContent = new CUIBool2(true, false);

      Box = new CUIDropDownBox(this);
      Append(Box);
      OnMouseDown += (CUIMouse m) => Toggle();
      Close();
    }

    public CUIDropDown(float? width, float? height) : this(null, null, width, height) { }

    public CUIDropDown(float? x, float? y, float? w, float? h) : this()
    {
      Relative = new CUINullRect(x, y, w, h);
    }
  }
}