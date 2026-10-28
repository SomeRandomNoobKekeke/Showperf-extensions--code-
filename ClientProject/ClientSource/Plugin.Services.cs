using Barotrauma;
using CursedUI;
using CUILibs;
using Microsoft.Xna.Framework;
using LightInject;
using System.Diagnostics;

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
        _ServiceContainer = new(new ContainerOptions { EnablePropertyInjection = false });
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
    ServiceContainer.RegisterSingleton<CustomPatchRepo>();
    ServiceContainer.RegisterSingleton<Settings>();
    ServiceContainer.RegisterSingleton<UltimateAssemblyAnalyzer3000>();
    ServiceContainer.RegisterSingleton<CallGraphBuilder>();
    ServiceContainer.RegisterSingleton<CaptureCore>();
    ServiceContainer.Register<ICallGraphSerializer, MetadataCallGraphSerializer>();
    ServiceContainer.RegisterSingleton<CallGraphSaver>();
    ServiceContainer.RegisterSingleton<CallGraphExplorer>();
    ServiceContainer.RegisterSingleton<PatchInstaller>();
    ServiceContainer.RegisterSingleton<StopwatchWrapper>();


    ServiceContainer.RegisterSingleton<CustomPatchesPage>();
    ServiceContainer.RegisterSingleton<MeasurementsPage>();

    ServiceContainer.RegisterSingleton<MethodsPage>();
    ServiceContainer.RegisterSingleton<TypeExplorerPage>();
    ServiceContainer.RegisterSingleton<CallGraphBrowserPage>();
    ServiceContainer.RegisterSingleton<MethodDetailsPage>();

    ServiceContainer.RegisterSingleton<ShowperfFrame>();
  }
}