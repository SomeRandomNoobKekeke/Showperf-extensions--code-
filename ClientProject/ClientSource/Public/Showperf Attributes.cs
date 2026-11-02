using Barotrauma;
using CursedUI;
using CUILibs;
using Microsoft.Xna.Framework;
using System.Reflection;
using HarmonyLib;

namespace ShowperfExtensions;

public class ShowperfPatchAttribute(string? id = null, string? description = null) : Attribute
{
  public string? ID { get; } = id;
  public string? Desctiption { get; } = description;
}

// Unfortunatelly you can't just pass MethodBase to Attribute, it's not allowed in Attribute params
// also expressions are not allowed in params
// .net docs suggest passing type and method name, but it'll ambiguate overloads, generics etc
// I'll just reuse MethodBaseParser
public class PatchMethodAttribute(string fullMethodName, HarmonyPatchType patchType = HarmonyPatchType.Prefix) : Attribute
{
  public MethodBase Target { get; } =
    MethodBaseParser.Parse(fullMethodName)
    ?? throw new ArgumentException($"PatchMethodAttribute couldn't find [{fullMethodName}], you sure it exists?");
  public HarmonyPatchType PatchType { get; } = patchType;
}