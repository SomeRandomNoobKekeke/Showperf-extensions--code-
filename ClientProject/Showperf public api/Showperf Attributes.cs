using Barotrauma;
using Microsoft.Xna.Framework;
using System.Reflection;
using HarmonyLib;

namespace ShowperfExtensions;


/// <summary>
/// Classes marked with this are considered custom patches  
/// They can have one or multiple PatchMethods  
/// `public static CUIComponent SettingsPage { get; }` prop that returns settings page
/// </summary>
public class ShowperfPatchAttribute(string? id = null, string? description = null) : Attribute;


/// <summary>
/// All these methods will be installed when you enable patch  
/// fullMethodName should be in this format:
/// Barotrauma.CharacterHealth.GetAffliction<T>(Identifier, Boolean)  
/// you can omit Barotrauma.
/// you can omit (Identifier, Boolean) if there's no overloads
/// </summary>
/// <param name="fullMethodName">like MapEntity.UpdateAll(Single, Camera)</param>
/// <param name="targetGameVersion">Just to throw warnings at you when patch is outdated. omit if don't care</param>
/// <param name="patchType"></param>
public class PatchMethodAttribute(
  string fullMethodName,
  string targetGameVersion,
  HarmonyPatchType patchType = HarmonyPatchType.Prefix
) : Attribute;
