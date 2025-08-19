using ei8.Cortex.Coding;
using ei8.Cortex.Coding.Persistence;
using ei8.Cortex.Library.Client.Out;
using Nancy.TinyIoc;

namespace ei8.Extensions.DependencyInjection.Coding.Persistence
{
    public static class TinyIoCContainerExtensions
    {
        public static void AddNetworkRepository(this TinyIoCContainer container, string cortexLibraryOutBaseUrl, int queryResultLimit, string appUserId)
        {
            container.Register<INetworkRepository>(
                (tic, npo) => new NetworkRepository(
                    container.Resolve<INeuronQueryClient>(),
                    cortexLibraryOutBaseUrl,
                    queryResultLimit,
                    appUserId
                )
            );
        }
    }
}
