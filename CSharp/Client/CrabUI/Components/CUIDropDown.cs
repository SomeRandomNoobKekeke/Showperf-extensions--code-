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
  //FIXME i'm broken
  public class CUIDropDown : CUIButton
  {
    #region ------------------ CUIDropDownOption ------------------
    public class Option : CUITextBlock
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

      public Option(string text, string value, CUIDropDown host) : base(text)
      {
        Host = host;
        Value = value;

        HoverColor = CUIPallete.Default.Tertiary.OffHover;
        TextColor = CUIPallete.Default.Tertiary.Text;

        Relative = new CUINullRect(0, null, 1, null);

        OnMouseDown += (CUIMouse m) =>
        {
          Host.Select(this);
          m.ClickConsumed = true;
        };
        OnMouseDown += (CUIMouse m) => SoundPlayer.PlayUISound(Host.ClickSound);

        OnTextChanged += () => { if (this == Host.Selected) Host.Text = Text; };
      }
    }
    #endregion
    #region ------------------ CUIDropDownBox ------------------
    public class CUIDropDownBox : CUIVerticalList
    {
      public CUIDropDown Host;

      public List<Option> Options = new List<Option>();

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



      public Option Add(object text, object value = null) => Add(text.ToString(), value?.ToString());
      public Option Add(string text, string value = null)
      {
        value ??= text;

        Option o = new Option(text, value, Host);

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
    #endregion
    #region ------------------ CUIDropDown ------------------


    public GUISoundType ClickSound { get; set; } = GUISoundType.Select;


    public CUIDropDownBox Box;
    public Option Selected;

    public Option Add(object text, object value = null) => Box.Add(text, value);
    public void Open() => Box.Open();
    public void Close() => Box.Close();
    public void Toggle() => Box.Toggle();

    public event Action<string> OnSelect;
    public Action<string> AddOnSelect { set { OnSelect += value; } }

    public void Select(object value) => Select(Box.Options.Find(o => o.Value == value.ToString()));
    public void Select(Option option)
    {
      if (option == null) return;
      Selected = option;
      OnSelect?.Invoke(Selected.Value);
      Text = option.Text;
    }

    internal override Vector2 AmIOkWithThisSize(Vector2 size)
    {
      Vector2 s = base.AmIOkWithThisSize(size);
      return new Vector2(size.X, s.Y);
    }


    public CUIDropDown() : base("CUIDropDown")
    {
      FitContent = new CUIBool2(true, false);

      Box = new CUIDropDownBox(this);
      Append(Box);
      OnMouseDown += (m) => Toggle();
      Close();
    }

    public CUIDropDown(float? width, float? height) : this(null, null, width, height) { }

    public CUIDropDown(float? x, float? y, float? w, float? h) : this()
    {
      Relative = new CUINullRect(x, y, w, h);
    }

    #endregion
  }
}