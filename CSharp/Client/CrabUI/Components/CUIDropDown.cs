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



      public CUIDropDownOption Add(string text, string value = null)
      {
        value ??= text;

        CUIDropDownOption o = new CUIDropDownOption(text, value, Host);

        Options.Add(o);
        Append(o);

        //Absolute = Absolute with { Height = Options.Count * Host.EnforcedOptionHeight };

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
        BackgroundColor = Host.BoxColor;
        OnMouseDown += (m) => Close();

        CUI.Main.OnMouseDown += (m) => Close();


        Close();
      }
    }


    public class CUIDropDownOption : CUITextBlock
    {
      public CUIDropDown Host;

      public string Value;

      protected override void Draw(SpriteBatch spriteBatch)
      {
        BackgroundColor = Host.OptionColor;
        if (MouseOver) BackgroundColor = Host.OptionHover;
        base.Draw(spriteBatch);
      }

      public CUIDropDownOption(string text, string value, CUIDropDown host) : base(text)
      {
        Host = host;
        Value = value;

        Relative = new CUINullRect(0, null, 1, null);
        Absolute = Absolute with { Height = Host.EnforcedOptionHeight };

        OnMouseDown += (CUIMouse m) => Host.Select(this);
        OnMouseDown += (CUIMouse m) => SoundPlayer.PlayUISound(Host.ClickSound);
      }
    }

    public GUISoundType ClickSound { get; set; } = GUISoundType.Select;
    public Color BoxColor = CUIColors.DropDownBox;
    public Color OptionColor = CUIColors.DropDownOption;
    public Color OptionHover = CUIColors.DropDownOptionHover;
    public float EnforcedOptionHeight = 21f;



    public CUIDropDownBox Box;
    public CUIDropDownOption Selected;

    public CUIDropDownOption Add(string text, string value = null) => Box.Add(text, value);
    public void Open() => Box.Open();
    public void Close() => Box.Close();
    public void Toggle() => Box.Toggle();

    public void Select(CUIDropDownOption option)
    {
      Selected = option;
      Text = option.Text;
      OnPropChanged();
    }


    public CUIDropDown() : base("CUIDropDown")
    {
      InactiveColor = CUIColors.DropDownInactive;
      MouseOverColor = CUIColors.DropDownHover;
      MousePressedColor = CUIColors.DropDownPressed;

      FitContent = new CUIBool2(true, false);

      Box = new CUIDropDownBox(this);
      Append(Box);
      OnMouseDown += (CUIMouse m) => Toggle();
      Close();

      Add("1");
      Add("2");
      Add("2313123123123");
      Add("4");


    }

    public CUIDropDown(float? width, float? height) : this(null, null, width, height) { }

    public CUIDropDown(float? x, float? y, float? w, float? h) : this()
    {
      Relative = new CUINullRect(x, y, w, h);
    }
  }
}