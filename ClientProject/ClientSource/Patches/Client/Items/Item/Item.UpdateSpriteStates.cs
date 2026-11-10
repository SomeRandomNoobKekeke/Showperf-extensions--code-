using ShowperfExtensions;
using System.Diagnostics;
using Barotrauma.Items.Components;
using static Barotrauma.MapEntity;
using CursedUI;

using FarseerPhysics;
using FarseerPhysics.Dynamics;
using CUILibs;
using Microsoft.Xna.Framework.Graphics;

namespace ShowperfPatches;

// it's useless
// [ShowperfPatch("Deep Item.UpdateSpriteStates")]
public static partial class DeepItemUpdateSpriteStates
{

  public static CUIComponent SettingsPage
  {
    get
    {
      var settings = new CUIDefault.VerticalBackPanel();

      settings["by id"] = new CUIDefault.ButtonGroupField()
      {
        Text = "Capture:",
        GroupName = "item draw capture mode",
        Options = ["By Prefab", "By ID"],
        SelectedOption = ById ? "By ID" : "By Prefab",
        OnSelected = (name) => ById = name == "By ID",
      };

      return settings;
    }
  }

  public static bool ById { get; set; }


  [PatchMethod("Item.UpdateSpriteStates", "1.15.28.0")]
  public static bool Item_UpdateSpriteStates_Replace(Item __instance, float deltaTime)
  {
    Item _ = __instance;

    DisposableStopwatch whole = DisposableStopwatch.StartNew();


    if (_.activeContainedSprite != null)
    {
      if (_.activeContainedSprite.DecorativeSpriteBehavior == ContainedItemSprite.DecorativeSpriteBehaviorType.HideWhenVisible)
      {
        foreach (DecorativeSprite decorativeSprite in _.Prefab.DecorativeSprites)
        {
          DecorativeSprite.State spriteState = _.spriteAnimState[decorativeSprite];
          spriteState.IsActive = false;
        }
        goto Exit; // return;
      }
    }
    else
    {
      foreach (ContainedItemSprite containedSprite in _.Prefab.ContainedSprites)
      {
        if (containedSprite.Sprite != _.activeSprite && containedSprite.DecorativeSpriteBehavior == ContainedItemSprite.DecorativeSpriteBehaviorType.HideWhenNotVisible)
        {
          foreach (DecorativeSprite decorativeSprite2 in _.Prefab.DecorativeSprites)
          {
            DecorativeSprite.State spriteState2 = _.spriteAnimState[decorativeSprite2];
            spriteState2.IsActive = false;
          }
          goto Exit; // return;
        }
      }
    }
    if (_.Prefab.DecorativeSpriteGroups.Count > 0)
    {
      DecorativeSprite.UpdateSpriteStates(_.Prefab.DecorativeSpriteGroups, _.spriteAnimState, (int)_.ID, deltaTime, new Func<PropertyConditional, bool>(_.ConditionalMatches));
    }
    foreach (Upgrade upgrade in _.Upgrades)
    {
      foreach (DecorativeSprite decorativeSprite3 in _.GetUpgradeSprites(upgrade))
      {
        DecorativeSprite.State spriteState3 = _.spriteAnimState[decorativeSprite3];
        spriteState3.IsActive = true;
        foreach (PropertyConditional conditional in decorativeSprite3.IsActiveConditionals)
        {
          if (!_.ConditionalMatches(conditional))
          {
            spriteState3.IsActive = false;
            break;
          }
        }
      }
    }
    foreach (Item containedItem in _.ContainedItems)
    {
      containedItem.UpdateSpriteStates(deltaTime);
    }

  Exit:

    whole.Stop();
    if (ById)
    {
      Capture.Ticks(_.ToString(), whole);
    }
    else
    {
      Capture.Ticks(_.Prefab.Identifier, whole);
    }

    return false;
  }
}