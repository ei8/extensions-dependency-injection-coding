using ei8.Cortex.Coding;
using Nancy.TinyIoc;
using System.Collections.Generic;

namespace ei8.Extensions.DependencyInjection.Coding
{
    public static class TinyIoCContainerExtensions
    {
        /// <summary>
        /// Registers the Read/Write Cache.
        /// </summary>
        /// <param name="container"></param>
        public static void AddReadWriteCache(this TinyIoCContainer container)
        {
            var nd = new Dictionary<CacheKey, Network>
            {
                { CacheKey.Write, new Network() },
                { CacheKey.Read, new Network() }
            };

            container.Register<INetworkDictionary<CacheKey>>(
                new NetworkDictionary<CacheKey>(nd)
            );
        }
    }
}
