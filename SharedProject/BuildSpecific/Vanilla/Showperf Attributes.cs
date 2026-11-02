using Barotrauma;
using Microsoft.Xna.Framework;
using System.Reflection;
using HarmonyLib;

namespace ShowperfExtensions;

// these are basically refs
// how it works: this is "in-memory" mod, it's compiled in game by LuaCsForBarotraumaPlugin
// it's compiled after ShowperfExtensions so real attributes will be there
// these definitions are not in ModConfig.xml so they won't be compiled
// but they are in SharedProject.projitems so ide could shut up about missing stuff
public class ShowperfPatchAttribute(string? id = null, string? description = null) : Attribute;
public class PatchMethodAttribute(string fullMethodName, HarmonyPatchType patchType = HarmonyPatchType.Prefix) : Attribute;
