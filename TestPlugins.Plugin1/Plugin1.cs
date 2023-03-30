using Microsoft.Extensions.DependencyInjection;
using TestPlugins.PluginInterface;

namespace TestPlugins.Plugin1
{
    public class Plugin1 : IPlugin
    {
        public void Init(IServiceCollection services)
        {
            Console.WriteLine("Plugin1 Initing.");

            services.AddTransient<IService1, Service1>();

            Console.WriteLine("Plugin1 Inited.");
        }

        public void Run(IServiceProvider serviceProvider)
        {
            var service1 = serviceProvider.GetRequiredService<Service1>();
        }
    }

    public interface IService1
    { }

    public class Service1 : IService1
    {
        public Service1()
        {
            Console.WriteLine("Service1 Ctor.");
        }
    }
}