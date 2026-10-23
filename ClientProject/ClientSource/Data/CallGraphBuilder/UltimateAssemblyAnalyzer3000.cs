using System.Reflection;
using HarmonyLib;
using System.Text;
using System.Text.Json;
using CUILibs;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Microsoft.Xna.Framework;


namespace ShowperfExtensions;

/// <summary>
/// Hardcoded to analyze Barotrauma.dll
/// </summary>
public class UltimateAssemblyAnalyzer3000
{
  public UltimateAssemblyAnalyzer3000()
  {
    BarotraumaCecilModule = AssemblyDefinition.ReadAssembly("Barotrauma.dll").MainModule;
    BarotraumaAssembly = typeof(Barotrauma.GameMain).Assembly;
    BarotraumaReflectionModule = BarotraumaAssembly.Modules.First();
  }



  public Assembly BarotraumaAssembly { get; }
  public Module BarotraumaReflectionModule { get; }
  public ModuleDefinition BarotraumaCecilModule { get; }


  public MethodBase ToMethodBase(MethodDefinition method)
    => BarotraumaReflectionModule.ResolveMethod(method.MetadataToken.ToInt32())!;

  public MethodDefinition ToMethodDefinition(MethodBase method)
    => (BarotraumaCecilModule.LookupToken(method.MetadataToken) as MethodDefinition)!;

  public IEnumerable<MethodBase> GetCalledMethods(MethodBase method)
    => GetCalledMethods(ToMethodDefinition(method)).Select(ToMethodBase);

  public IEnumerable<MethodDefinition> GetCalledMethods(MethodDefinition method)
    => _GetCalledMethods(method).Distinct();

  private IEnumerable<MethodDefinition> _GetCalledMethods(MethodDefinition method)
  {
    if (!method.HasBody) yield break;

    foreach (Instruction instruction in method.Body.Instructions)
    {
      if (instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt)
      {
        MethodDefinition calledMethod = (instruction.Operand as MethodReference)!.Resolve();

        if (calledMethod is null) continue;
        if (calledMethod.IsGetter) continue;
        if (calledMethod.IsSetter) continue;
        if (calledMethod.IsAddOn) continue;
        if (calledMethod.IsRemoveOn) continue;

        if (calledMethod.Module != BarotraumaCecilModule) continue;

        yield return calledMethod;
      }
    }
  }


  public void PrintInstructions(MethodBase method)
  {
    Logger.Default.Log($"\n{method}:");
    foreach (Instruction instruction in ToMethodDefinition(method).Body.Instructions)
    {
      if (instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt)
      {
        Logger.Default.Print(instruction, Color.Lime);
        continue;
      }

      Logger.Default.Print(instruction, Color.White);
    }
  }
  public IEnumerable<Instruction> GetInstructions(MethodBase method)
    => ToMethodDefinition(method).Body.Instructions;
}