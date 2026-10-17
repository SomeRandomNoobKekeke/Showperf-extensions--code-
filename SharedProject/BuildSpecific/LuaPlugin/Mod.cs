using Barotrauma;
using Microsoft.Xna.Framework;
using Barotrauma.LuaCs;

namespace ShowperfExtensions
{
  public partial class Mod : IAssemblyPlugin
  {
    public void Initialize() => Init();
    public void OnLoadCompleted() { }
    public void PreInitPatching() { }
  }

}

