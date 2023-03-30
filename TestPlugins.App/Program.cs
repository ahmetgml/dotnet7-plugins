using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using TestPlugins.PluginInterface;

namespace TestPlugins.App
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("TestPlugins.App Starting.");

            using (IHost host = CreateHostBuilder(args).Build())
            {
                foreach (var plugin in _plugins)
                {
                    plugin.Run(host.Services);
                }

                await host.RunAsync().ConfigureAwait(false);
            }

            Console.WriteLine("TestPlugins.App Ending.");
        }

        private static readonly HashSet<IPlugin> _plugins = new();

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var assemblies = GetPlugins();

            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    foreach (var assembly in assemblies)
                    {
                        var obj = Activator.CreateInstance(assembly.Value) as IPlugin;
                        if (obj != null)
                        {
                            obj.Init(services);
                            _plugins.Add(obj);
                        }
                    }
                });
        }

        private static Dictionary<Assembly, Type> GetPlugins()
        {
            var ipluginType = typeof(IPlugin);
            var pluginDir = Path.Combine(Environment.CurrentDirectory, "Plugins");
            var pluginsDlls = Directory.GetFiles(pluginDir, "*.dll", SearchOption.AllDirectories).ToList();
            return pluginsDlls.Select(dll =>
            {
                return ReflectionsExcCatch(() =>
                {
                    var alc = new AssemblyLoadContext(dll);
                    var asm = alc.LoadFromAssemblyPath(dll);
                    var types = asm.GetTypes();
                    var ipluginTypes = types.Where(t => ipluginType.IsAssignableFrom(t) && t != ipluginType);
                    return new KeyValuePair<Assembly, Type?>(
                        asm,
                        ipluginTypes.FirstOrDefault());
                });
            })
            .Where(x => x.Value != null)
            .ToDictionary(x => x.Key, y => y.Value)!;
        }

        private static KeyValuePair<Assembly, Type?> ReflectionsExcCatch(Func<KeyValuePair<Assembly, Type?>> action)
        {
            try
            {
                return action();
            }
            catch (ReflectionTypeLoadException ex)
            {
                var sb = new StringBuilder();
                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                    if (exFileNotFound != null)
                    {
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                string errorMessage = sb.ToString();
                Debug.WriteLine(errorMessage);
                throw;
            }
        }
    }
}