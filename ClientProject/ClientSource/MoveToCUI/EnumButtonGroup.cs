using ShowperfExtensions;
using System.Diagnostics;
using Barotrauma.Items.Components;
using static Barotrauma.MapEntity;
using CursedUI;
using HarmonyLib;
namespace ShowperfPatches;


public class EnumButtonGroupField<T> : CUIDefault.ButtonGroupField where T : Enum
{
  private static int MaxID = 0;

  public new Action<T> OnSelected { set { Selected += value; } }
  public new event Action<T> Selected;

  public new T SelectedOption
  {
    get => (T)(object)ButtonGroup.Children.IndexOf(Group.Current);
    set
    {
      Group.Select((ButtonGroup.Children[(int)(object)value] as CUIRadioButton)!);
    }
  }

  public EnumButtonGroupField(string text) : this() { Text = text; }
  public EnumButtonGroupField()
  {
    Options = Enum.GetNames(typeof(T)).ToList();

    GroupName = $"EnumButtonGroup<{typeof(T)}>:{MaxID++}";
    Group.Selected += (btn) =>
    {
      int i = ButtonGroup.Children.IndexOf(btn);
      Selected?.Invoke((T)(object)i);
    };
  }
}