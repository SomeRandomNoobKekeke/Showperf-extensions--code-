using Barotrauma;
using CursedUI;
using CUILibs;
using Microsoft.Xna.Framework;
using LightInject;
using System.Diagnostics;
using System.IO;

namespace ShowperfExtensions;

public partial class Plugin
{
  private static ServiceContainer _ServiceContainer;
  public static ServiceContainer ServiceContainer
  {
    get
    {
      if (_ServiceContainer is null)
      {
        _ServiceContainer = new(new ContainerOptions
        {
          EnablePropertyInjection = false,
          // LogFactory = (type) => logEntry => Logger.Default.Log($"LI| {Logger.White(logEntry.Message)}"),
        });
        RegisterServices();
      }

      return _ServiceContainer;
    }
    set => _ServiceContainer = value;
  }

  /// <summary>
  /// ServiceLocator
  /// </summary>
  public static T Get<T>() => ServiceContainer.GetInstance<T>();

  private static void RegisterServices()
  {
    ServiceContainer.RegisterSingleton<Plugin>();
    ServiceContainer.RegisterSingleton<CustomPatchRepo>();
    ServiceContainer.RegisterSingleton<CustomPatchRepo.EntryInstaller>();

    ServiceContainer.RegisterSingleton<PluginSettings>(
      (factory) => new PluginSettings(Path.Combine("Data", "Mods", ModInfo.Name, "Settings.json"))
    );
    ServiceContainer.RegisterSingleton<UltimateAssemblyAnalyzer3000>();
    ServiceContainer.RegisterSingleton<CallGraphBuilder>();
    ServiceContainer.RegisterSingleton<CaptureCore>();
    ServiceContainer.Register<ICallGraphSerializer, MetadataCallGraphSerializer>();
    ServiceContainer.RegisterSingleton<CallGraphProvider>();
    ServiceContainer.RegisterSingleton<CallGraphExplorer>();
    ServiceContainer.RegisterSingleton<PatchInstaller>();
    ServiceContainer.RegisterSingleton<StopwatchWrapper>();
    ServiceContainer.RegisterSingleton<MethodBookmarksList>();



    ServiceContainer.RegisterSingleton<PatchesPage>();
    ServiceContainer.RegisterSingleton<PatchesPage.CustomPatchesPage>();
    ServiceContainer.RegisterSingleton<PatchesPage.InstalledPatchesPage>();

    ServiceContainer.RegisterSingleton<MeasurementsPage>();
    ServiceContainer.RegisterSingleton<SettingsPage>();

    ServiceContainer.RegisterSingleton<MethodsPage>();
    ServiceContainer.RegisterSingleton<TypeExplorerPage>();
    ServiceContainer.RegisterSingleton<CallGraphBrowserPage>();
    ServiceContainer.RegisterSingleton<BookmarkedMethodsPage>();

    ServiceContainer.RegisterSingleton<ShowperfFrame>();
  }
}