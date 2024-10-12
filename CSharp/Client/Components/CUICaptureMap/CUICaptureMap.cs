#define DEBUG

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using HarmonyLib;
using CrabUI;
using System.IO;

namespace ShowPerfExtensions
{
  public partial class Mod : IAssemblyPlugin
  {
    //TODO this is dead end, there's no way to expand it, find better view
    // mb add zoom?
    //TODO also this should be in xml
    public partial class CUICaptureMap : CUIMap
    {

      public void Fill()
      {
        Append(new CUITextBlock("Client") { Fixed = true, });

#if DEBUG
        Append(new CUIButton("Dump")
        {
          Fixed = true,
          Anchor = new CUIAnchor(CUIAnchorType.RightTop),
          AddOnMouseDown = (e) =>
          {
            int Btnprecision = 1;
            int Bendprecision = 5;
            Map.Children.ForEach(c =>
            {
              if (c is MapButton)
              {
                c.Absolute = c.Absolute with
                {
                  Position = new Vector2(
                    (float)Math.Round((c.Absolute.Left ?? 0) / Btnprecision) * Btnprecision,
                    (float)Math.Round((c.Absolute.Top ?? 0) / Btnprecision) * Btnprecision
                  )
                };
              }
              if (c is MapBend)
              {
                c.Absolute = c.Absolute with
                {
                  Position = new Vector2(
                    (float)Math.Round((c.Absolute.Left ?? 0) / Bendprecision) * Bendprecision,
                    (float)Math.Round((c.Absolute.Top ?? 0) / Bendprecision) * Bendprecision
                  )
                };
              }
            });

            string s = String.Join('\n', Map.Children.Select(c =>
            {
              if (c is MapButton btn)
              {
                string state = btn.CState == null ? "" : $", Capture.{btn.CState.Category}";

                return $"Add(\"{c.AKA}\", new MapButton({c.Absolute.Left}, {c.Absolute.Top}, \"{c.AKA}\"{state}));";
              }
              if (c is MapBend)
              {
                string disabled = c.Disabled ? "true" : "false";
                return $"Add(\"{c.AKA}\", new MapBend({c.Absolute.Left}, {c.Absolute.Top}, {disabled}));";
              }
              return "";
            }));
            File.WriteAllText("1111CUIMapDump.txt", s);
          },
        });
#endif

        //Autogenerated, lol
        Add("GameScreen", new MapButton(50, 20, "GameScreen"));
        Add("Update", new MapButton(20, 50, "Update"));
        Add("Draw", new MapButton(175, 20, "Draw"));
        Add("SubmarineBend", new MapBend(270, 27, true));
        Add("Submarine", new MapButton(247, 38, "Submarine"));
        Add("DrawFront", new MapButton(215, 59, "DrawFront"));
        Add("DrawBack", new MapButton(283, 59, "DrawBack"));
        Add("MapEntityDrawing", new MapButton(229, 81, "MapEntityDrawing", Capture.MapEntityDrawing));
        Add("LevelObjectManagerBend", new MapBend(215, 122, true));
        Add("LevelObjectManager", new MapButton(235, 115, "LevelObjectManager"));
        Add("DrawObjects", new MapButton(258, 147, "DrawObjects", Capture.LevelObjectsDrawing));
        Add("MapEntity.UpdateAll", new MapButton(7, 108, "MapEntity.UpdateAll"));
        Add("MapEntity.UpdateAllOutBend", new MapBend(5, 155, false));
        Add("ItemUpdateInBend", new MapBend(30, 250, false));
        Add("HullUpdateInBend", new MapBend(30, 155, false));
        Add("GapUpdateInBend", new MapBend(30, 185, false));
        Add("StructureUpdateInBend", new MapBend(30, 220, false));
        Add("ItemUpdateOutBend", new MapBend(150, 250, false));
        Add("HullUpdateOutBend", new MapBend(150, 155, false));
        Add("GapUpdateOutBend", new MapBend(150, 185, false));
        Add("StructureUpdateOutBend", new MapBend(150, 220, false));
        Add("ItemUpdate", new MapButton(50, 245, "ItemUpdate", Capture.ItemUpdate));
        Add("HullUpdate", new MapButton(55, 150, "HullUpdate", Capture.HullUpdate));
        Add("GapUpdate", new MapButton(55, 180, "GapUpdate", Capture.GapUpdate));
        Add("StructureUpdate", new MapButton(45, 215, "StructureUpdate", Capture.StructureUpdate));
        Add("WholeSubmarineUpdate", new MapButton(161, 281, "WholeSubmarineUpdate", Capture.WholeSubmarineUpdate));
        Add("WholeSubmarineBend", new MapBend(180, 220, false));
        Add("ItemComponentsUpdate", new MapButton(17, 280, "ItemComponentsUpdate", Capture.ItemComponentsUpdate));
        Add("LevelRendererInBend", new MapBend(205, 150, true));
        Add("LevelRenderer", new MapButton(245, 185, "LevelRenderer"));
        Add("DrawBackground", new MapButton(238, 211, "DrawBackground", Capture.LevelMisc));
        Add("Character.UpdateAll", new MapButton(70, 75, "Character.UpdateAll", Capture.CharactersUpdate));

        ConnectTo(this["GameScreen"],
          ConnectTo(this["Draw"],
            ConnectTo(this["LevelRendererInBend"],
              ConnectTo(this["LevelRenderer"],
                this["DrawBackground"]
              )
            ),
            ConnectTo(this["SubmarineBend"],
              ConnectTo(this["Submarine"],
                ConnectTo(this["DrawFront"], this["MapEntityDrawing"]),
                ConnectTo(this["DrawBack"], this["MapEntityDrawing"])
              )
            ),
            ConnectTo(this["LevelObjectManagerBend"],
              ConnectTo(this["LevelObjectManager"],
                this["DrawObjects"]
              )
            )
          ),


          //Honesty, i'm surprised that you can just stack them like this
          ConnectTo(this["Update"],
            this["Character.UpdateAll"],
            ConnectTo(this["MapEntity.UpdateAll"],
              ConnectTo(this["MapEntity.UpdateAllOutBend"],
                ConnectTo(this["ItemUpdateInBend"],
                  ConnectTo(this["ItemUpdate"],
                    ConnectTo(this["ItemUpdateOutBend"], this["WholeSubmarineBend"]),
                    this["ItemComponentsUpdate"]
                  )
                ),
                ConnectTo(this["HullUpdateInBend"],
                  ConnectTo(this["HullUpdate"],
                    ConnectTo(this["HullUpdateOutBend"], this["WholeSubmarineBend"])
                  )
                ),
                ConnectTo(this["GapUpdateInBend"],
                  ConnectTo(this["GapUpdate"],
                    ConnectTo(this["GapUpdateOutBend"], this["WholeSubmarineBend"])
                  )
                ),
                ConnectTo(this["StructureUpdateInBend"],
                  ConnectTo(this["StructureUpdate"],
                    ConnectTo(this["StructureUpdateOutBend"],
                      ConnectTo(this["WholeSubmarineBend"], this["WholeSubmarineUpdate"])
                    )
                  )
                )
              )
            )
          )
        );
      }


      public CUICaptureMap()
      {
        BackgroundColor = Color.Transparent;
        BorderColor = Color.Transparent;
        OnDClick += (e) => ChildrenOffset = Vector2.Zero;
      }

      public CUICaptureMap(float? x = null, float? y = null, float? w = null, float? h = null) : this()
      {
        Relative = new CUINullRect(x, y, w, h);
      }
    }
  }
}