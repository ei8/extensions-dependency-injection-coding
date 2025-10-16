using ei8.Cortex.Coding;
using ei8.Cortex.Coding.d23.neurULization;
using ei8.Cortex.Coding.d23.neurULization.Persistence;
using ei8.Cortex.Coding.Persistence;
using ei8.Cortex.IdentityAccess.Client.Out;
using ei8.EventSourcing.Client;
using Nancy.TinyIoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ei8.Extensions.DependencyInjection.Coding.d23.neurULization.Persistence
{
    public static class TinyIoCContainerExtensions
    {

        public static async Task<(bool initialized, bool registered)> AddMirrorsAsync(
            this TinyIoCContainer container,
            IEnumerable<object> initMirrorKeys
        ) => await container.AddMirrorsAsync(
            initMirrorKeys, 
            false, 
            Guid.Empty
        );

        /// <summary>
        /// Registers Mirrors.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="initMirrorKeys"></param>
        /// <param name="shouldInitializeMissingMirrors"></param>
        /// <param name="userNeuronId"></param>
        /// <returns>A boolean tuple indicating whether mirrors were (1) initialized or (2) registered successfully.</returns>
        public static async Task<(bool initialized, bool registered)> AddMirrorsAsync(
            this TinyIoCContainer container,
            IEnumerable<object> initMirrorKeys,
            bool shouldInitializeMissingMirrors,
            Guid userNeuronId
        )
        {
            bool registered = false;

            var mirrorRepository = container.Resolve<IMirrorRepository>();
            var missingInitMirrorConfigs = await mirrorRepository.GetAllMissingAsync(initMirrorKeys);

            bool initialized = shouldInitializeMissingMirrors &&
                missingInitMirrorConfigs.Any() &&
                await TinyIoCContainerExtensions.InitializeMirrors(
                    container.Resolve<ITransaction>(),
                    userNeuronId,
                    mirrorRepository,
                    missingInitMirrorConfigs.Select(mimc => mimc.Key)
                );

            if (!initialized)
            {
                IMirrorSet mirrorSet = null;
                if ((mirrorSet = await mirrorRepository.CreateMirrorSet()) != null)
                {
                    container.Register(mirrorSet);
                    registered = true;
                }
            }

            return (initialized, registered);
        }

        private static async Task<bool> InitializeMirrors(
            ITransaction transaction, 
            Guid userNeuronId, 
            IMirrorRepository mirrorRepository,
            IEnumerable<string> keys = null
            )
        {
            await transaction.BeginAsync(userNeuronId);
            bool initialized = await mirrorRepository.Initialize(keys);
            await transaction.CommitAsync();

            return initialized;
        }

        /// <summary>
        /// Registers a GrannyService.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="identityAccessOutBaseUrl"></param>
        /// <param name="appUserId"></param>
        public static void AddGrannyService(this TinyIoCContainer container, string identityAccessOutBaseUrl, string appUserId)
        {
            container.Register<IGrannyService>(
                (tic, npo) => new GrannyService(
                    container.Resolve<IServiceProvider>(),
                    container.Resolve<INetworkRepository>(),
                    container.Resolve<INetworkDictionary<string>>(),
                    container.Resolve<ITransaction>(),
                    container.Resolve<INetworkTransactionService>(),
                    container.Resolve<IValidationClient>(),
                    identityAccessOutBaseUrl,
                    appUserId
                )
            );
        }
    }
}
