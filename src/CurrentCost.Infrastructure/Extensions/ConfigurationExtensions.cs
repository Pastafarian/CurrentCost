using Microsoft.Extensions.Configuration;

namespace CurrentCost.Infrastructure.Extensions
{
    public static class ConfigurationExtensions
    {
        public static T GetSafely<T>(this IConfiguration configuration)
        {
            var result = configuration.Get<T>();

            return result == null ? throw new Exception($"Error getting value {typeof(T)} from configuration") : result;
        }
    }
}
