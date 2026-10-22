using System.Reflection;
using HarmonyLib;
using System.Text;
using System.Text.Json;
using CUILibs;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Microsoft.Xna.Framework;


namespace ShowperfExtensions;

public class UltimateAssemblyAnalyzer3000
{
  public UltimateAssemblyAnalyzer3000() : this(
    typeof(Barotrauma.GameMain).Assembly,
    "Barotrauma.dll"
  )
  { }
  public UltimateAssemblyAnalyzer3000(Assembly assembly, string path)
  {
    CecilModule = AssemblyDefinition.ReadAssembly(path).MainModule;
    Assembly = assembly;
    ReflectionModule = assembly.Modules.First();
  }


  public Assembly Assembly { get; }
  public Module ReflectionModule { get; }
  public ModuleDefinition CecilModule { get; }


  public MethodBase ToMethodBase(MethodDefinition method)
    => ReflectionModule.ResolveMethod(method.MetadataToken.ToInt32())!;

  public MethodDefinition ToMethodDefinition(MethodBase method)
    => (CecilModule.LookupToken(method.MetadataToken) as MethodDefinition)!;

  public IEnumerable<MethodBase> GetCalledMethods(MethodBase method)
    => GetCalledMethods(ToMethodDefinition(method)).Select(ToMethodBase);

  public IEnumerable<MethodDefinition> GetCalledMethods(MethodDefinition method)
  {
    foreach (Instruction instruction in method.Body.Instructions)
    {
      if (instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt)
      {
        MethodDefinition calledMethod = (instruction.Operand as MethodReference)!.Resolve();

        if (calledMethod.IsGetter) continue;
        if (calledMethod.IsSetter) continue;
        if (calledMethod.IsAddOn) continue;
        if (calledMethod.IsRemoveOn) continue;


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