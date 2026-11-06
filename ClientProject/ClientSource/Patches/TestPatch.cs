using ShowperfExtensions;
using System.Diagnostics;
using Barotrauma.Items.Components;
using static Barotrauma.MapEntity;
using CursedUI;
namespace ShowperfPatches;

// [ShowperfPatch]
public class TestPatch
{
  [PatchMethod("MapEntity.UpdateAll")]
  public static void UpdateAll(float deltaTime, Camera cam)
  {
    for (int i = 1; i <= 100; i++)
    {
      Capture.Ticks(i.ToString(), 1);
    }
  }
}