using System;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;


using Barotrauma;
using HarmonyLib;

using Barotrauma.Abilities;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
#if SERVER
using System.Text;
#endif


namespace ShowPerfExtensions
{
  public partial class Plugin
  {
    public partial class CharacterPatch
    {
      public static bool Character_UpdateInteractablesInRange_Replace(Character __instance)
      {
        if (Showperf == null || !Showperf.Revealed || !UpdateInteractablesInRangeState.IsActive) return true;
        Capture.Update.EnsureCategory(UpdateInteractablesInRangeState);
        Stopwatch sw = new Stopwatch();

        Character _ = __instance;



        // keep two lists to detect changes to the current state of interactables in range
        _.previousInteractablesInRange.Clear();
        _.previousInteractablesInRange.AddRange(_.interactablesInRange);

        _.interactablesInRange.Clear();

        //use the list of visible entities if it exists
        var entityList = Submarine.VisibleEntities ?? Item.ItemList;


        foreach (MapEntity entity in entityList)
        {
          if (entity is not Item item) { continue; }

          if (item.body != null && !item.body.Enabled) { continue; }

          if (item.ParentInventory != null) { continue; }

          if (item.Prefab.RequireCampaignInteract &&
              item.CampaignInteractionType == CampaignMode.InteractionType.None)
          {
            continue;
          }

          if (Screen.Selected is SubEditorScreen { WiringMode: true } &&
              item.GetComponent<ConnectionPanel>() == null)
          {
            continue;
          }
          sw.Restart();
          if (_.CanInteractWith(item))
          {
            _.interactablesInRange.Add(item);
          }
          sw.Stop();
          Capture.Update.AddTicks(sw.ElapsedTicks, UpdateInteractablesInRangeState, "CanInteractWith");
        }


        sw.Restart();
        if (!_.interactablesInRange.SequenceEqual(_.previousInteractablesInRange))
        {
          InteractionLabelManager.RefreshInteractablesInRange(_.interactablesInRange);
        }
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, UpdateInteractablesInRangeState, "SequenceEqual");


        return false;
      }

    }
  }
}