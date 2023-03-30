using Microsoft.Extensions.DependencyInjection;

namespace TestPlugins.PluginInterface
{
    public interface IPlugin
    {
        void Init(IServiceCollection services);

        void Run(IServiceProvider serviceProvider);
    }
}