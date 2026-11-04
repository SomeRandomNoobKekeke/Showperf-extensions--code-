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

  public PluginSettings PluginSettings { get; } = Plugin.Get<PluginSettings>();

  public CUIPages Pages { get; }

  public PatchesPage PatchesPage { get; } = Plugin.Get<PatchesPage>();
  public MethodsPage MethodsPage { get; } = Plugin.Get<MethodsPage>();
  public MeasurementsPage MeasurementsPage { get; } = Plugin.Get<MeasurementsPage>();
  public SettingsPage SettingsPage { get; } = Plugin.Get<SettingsPage>();

  public CUITextBlock FooterTextBlock { get; }
  public string FooterText
  {
    get => FooterTextBlock.Text;
    set => FooterTextBlock.Text = value;
  }

  public Logger_Part Logger { get; }


  private CUIComponent PageButtons;
  private CUIButton OpenButton;

  public void Minimize()
  {
    TargetMainComponent["open showperf"] = OpenButton;
    Close();
  }

  public ShowperfFrame() : base("Showperf", 500, 600)
  {
    Logger = new(this);

    TargetMainComponent = PluginSettings.FrameAboveGUI ? CUI.TopMain : CUI.Main;
    Anchor = CUIAnchor.RightCenter;
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


    using (new CUIContextStyle<CUIIconButton>(btn => btn.Margin = new CUISizes(left: 4, right: 4)))
    {
      this["layout"]["handle"]["minimize", 1] = new CUIIconButton(CUISprite.MinusIcon)
      {
        OnClick = Minimize,
      };
      this["layout"]["handle"]["attachtoright", 1] = new CUIIconButton(CUISprite.AttachToRightIcon)
      {
        OnClick = () =>
        {
          Absolute = new CUINullRect(0, 0, 500, 600);
          Anchor = CUIAnchor.RightCenter;
        },
      };
      this["layout"]["handle"]["attachtocenter", 1] = new CUIIconButton(CUISprite.AttachToCenterIcon)
      {
        OnClick = () =>
        {
          Absolute = new CUINullRect(0, 0, 800, 800);
          Anchor = CUIAnchor.Center;
        },
      };

      this["layout"]["handle"]["settings", 1] = new CUIIconButton(CUISprite.GearIcon)
      {
        OnClick = () =>
        {
          Pages.OpenPage(SettingsPage);
          //HACK mb if there was some CUIRadioIconButton it could be more natural
          CUIRadioButton.RadioGroup.GetOrCreate("ShowperfFrame pages").ClearSelection();
        },
      };
    }

    this["layout"]["handle"]["reloadlua", 1] = new CUIDefault.ReloadLuaButton();

    this["layout"]["page buttons"] = PageButtons = new CUIDefault.HorizontalPanel();
    this["layout"]["main"] = Pages = new CUIPages()
    {
      Flex = 1,
      ConsumeMouseEvents = true,
      OnPageOpened = (page) => Logger.Log(""),
    };

    using (new CUIContextStyle<CUIRadioButton>(btn =>
    {
      btn.Flex = 1;
      btn.GroupName = "ShowperfFrame pages";
    }))
    {
      this["layout"]["page buttons"]["Measurements"] = new CUIRadioButton("Ticks")
      {
        OnSelected = () => Pages.OpenPage(MeasurementsPage),
      };
      this["layout"]["page buttons"]["Patches"] = new CUIRadioButton("Patches")
      {
        OnSelected = () => Pages.OpenPage(PatchesPage),
      };
      this["layout"]["page buttons"]["Methods"] = new CUIRadioButton("Methods")
      {
        OnSelected = () => Pages.OpenPage(MethodsPage),
      };
    }

    this["layout"]["footer"] = new CUIDefault.HorizontalBackPanel()
    {
      Scrollable = true,
      Padding = new CUISizes(left: ResizeHandle.DefaultSize.X, right: ResizeHandle.DefaultSize.X),
    };
    this["layout"]["footer"]["text"] = FooterTextBlock = new CUITextBlock();

    DeepPalette = Plugin.Palettes.Primary;

    PageButtons.Get<CUIRadioButton>("Methods").Select();


  }
}