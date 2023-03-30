using Microsoft.Extensions.DependencyInjection;
using TestPlugins.PluginInterface;

namespace TestPlugins.Plugin2
{
    public class Plugin2 : IPlugin
    {
        public void Init(IServiceCollection services)
        {
            Console.WriteLine("Plugin2 Initing.");

            services.AddTransient<IService2, Service2>();

            Console.WriteLine("Plugin2 Inited.");
        }

        public void Run(IServiceProvider serviceProvider)
        {
            var service2 = serviceProvider.GetRequiredService<Service2>();
        }
    }

    public interface IService2
    { }

    public class Service2 : IService2
    {
        public Service2()
        {
            Console.WriteLine("Service2 Ctor.");
        }
    }
}