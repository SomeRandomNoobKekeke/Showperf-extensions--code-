using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace ShowperfExtensions;

public partial class ShowperfFrame : CUIDefault.Frame
{
  // public static ICUIStyle DefaultStyle => new CUIDefaultStyle<ShowperfFrame>((c) =>
  // {
  //   c.Background.Color = c.Palette["back"].To(Color.Transparent, 0.2f);
  // });



  public CUIPages Pages { get; }

  public CustomPatchesPage CustomPatchesPage { get; } = Plugin.Get<CustomPatchesPage>();
  public MethodsPage MethodsPage { get; } = Plugin.Get<MethodsPage>();
  public MeasurementsPage MeasurementsPage { get; } = Plugin.Get<MeasurementsPage>();

  private CUIComponent PageButtons;

  private CUIButton OpenButton;

  public void Minimize()
  {
    TargetMainComponent["open showperf"] = OpenButton;
    Close();
  }

  public ShowperfFrame() : base("Showperf", 600, 600)
  {
    TargetMainComponent = CUI.Main;
    Anchor = CUIAnchor.Center;
    OnOpen += DeepRefresh;
    ResizableBoth = true;

    OpenButton = new CUIButton("SHOWPERF")
    {
      Font = CUIFont.MonospacedFont,
      Anchor = CUIAnchor.RightCenter,
      Vertical = true,
      Borders = new CUISizes(1, 0, 1, 1),
      OnClick = () =>
      {
        OpenButton.RemoveSelf();
        Open();
      },
      Style = (btn) => btn.MasterColor = new Color(127, 127, 127, 127),
    };


    this["layout"]["handle"]["minimize", 1] = new CUIIconButton(CUISprite.MinusIcon)
    {
      Margin = new CUISizes(left: 4, right: 4),
      OnClick = Minimize,
    };
    this["layout"]["handle"]["attachtoright", 1] = new CUIIconButton(CUISprite.AttachToRightIcon)
    {
      Margin = new CUISizes(left: 4, right: 4),
      OnClick = () =>
      {
        Absolute = new CUINullRect(0, 0, 400, 600);
        Anchor = CUIAnchor.RightCenter;
      },
    };
    this["layout"]["handle"]["attachtocenter", 1] = new CUIIconButton(CUISprite.AttachToCenterIcon)
    {
      Margin = new CUISizes(left: 4, right: 4),
      OnClick = () =>
      {
        Absolute = new CUINullRect(0, 0, 800, 800);
        Anchor = CUIAnchor.Center;
      },
    };

    this["layout"]["handle"]["reloadlua", 1] = new CUIDefault.ReloadLuaButton();

    this["layout"]["page buttons"] = PageButtons = new CUIDefault.HorizontalPanel();
    this["layout"]["main"] = Pages = new CUIPages()
    {
      Flex = 1,
      ConsumeMouseEvents = true,
    };

    using (new CUIContextStyle<CUIRadioButton>(btn =>
    {
      btn.Flex = 1;
      btn.GroupName = "ShowperfFrame pages";
      btn.CullChildren = true;
    }))
    {
      this["layout"]["page buttons"]["Measurements"] = new CUIRadioButton("Measurements")
      {
        OnSelected = () => Pages.OpenPage(MeasurementsPage),
      };
      this["layout"]["page buttons"]["Custom Patches"] = new CUIRadioButton("Patches")
      {
        OnSelected = () => Pages.OpenPage(CustomPatchesPage),
      };
      this["layout"]["page buttons"]["Methods"] = new CUIRadioButton("Methods")
      {
        OnSelected = () => Pages.OpenPage(MethodsPage),
      };
    }

    // this["layout"]["footer"] = new CUIDefault.HorizontalBackPanel()
    // {
    //   Absolute = new CUINullRect(h: ResizeHandle.DefaultSize.Y),
    // };

    PageButtons.Get<CUIRadioButton>("Methods").Select();


  }
}