// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Infrastructure
{
    /// <summary>
    ///     Provides Entity Framework specific APIs for configuring services in an <see cref="IServiceCollection" />.
    ///     These APIs are usually accessed by calling
    ///     <see cref="EntityFrameworkServiceCollectionExtensions.AddEntityFramework(IServiceCollection, IConfiguration)" />
    ///     and then chaining API calls on the returned <see cref="EntityFrameworkServicesBuilder" />.
    /// </summary>
    public class EntityFrameworkServicesBuilder : IAccessor<IServiceCollection>, IAccessor<IConfiguration>
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly IConfiguration _configuration;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityFrameworkServicesBuilder" /> class.
        /// </summary>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> being configured. </param>
        /// <param name="configuration">
        ///     The configuration for the application. This configuration may contain Entity Framework specific settings
        ///     which will be passed into context instances that are created by dependency injection.
        /// </param>
        public EntityFrameworkServicesBuilder(
            [NotNull] IServiceCollection serviceCollection,
            [CanBeNull] IConfiguration configuration = null)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            _serviceCollection = serviceCollection;
            _configuration = configuration;
        }

        /// <summary>
        ///     Registers the given context as a service in the <see cref="IServiceCollection" />. 
        ///     You use this method when using dependency injection in your application, such as with ASP.NET.
        ///     For more information on setting up dependency injection, see http://go.microsoft.com/fwlink/?LinkId=526890.
        /// </summary>
        /// <remarks>
        ///     This method will ensure services that the context uses are resolved from the 
        ///     <see cref="IServiceProvider" /> and any Entity Framework configuration 
        ///     found in the configuration passed to <see cref="EntityFrameworkServiceCollectionExtensions.AddEntityFramework" /> 
        ///     extension method will be honored.
        /// </remarks>
        /// <typeparam name="TContext"> The type of context to be registered. </typeparam>
        /// <param name="optionsAction">
        ///     <para>
        ///         An optional action to configure the <see cref="DbContextOptions" /> for the context. This provides an
        ///         alternative to performing configuration of the context by overriding the 
        ///         <see cref="DbContext.OnConfiguring" /> method in your derived context.
        ///     </para>
        ///     <para>
        ///         If an action is supplied here, the <see cref="DbContext.OnConfiguring" /> method will still be run if it has
        ///         been overridden on the derived context. <see cref="DbContext.OnConfiguring" /> configuration will be applied 
        ///         in addition to configuration performed here.
        ///     </para>
        ///     <para>
        ///         You do not need to expose a constructor parameter for the <see cref="DbContextOptions" /> to be passed to the
        ///         context. If you choose to expose a constructor parameter, you must type it as the generic
        ///         <see cref="DbContextOptions{T}" /> as that is the type that will be registered in the 
        ///         <see cref="IServiceCollection" /> (in order to support multiple context types being registered in the 
        ///         same <see cref="IServiceCollection" />).
        ///     </para>
        /// </param>
        /// <returns>
        ///     A builder that allows further Entity Framework specific setup of the <see cref="IServiceCollection" />.
        /// </returns>
        public virtual EntityFrameworkServicesBuilder AddDbContext<TContext>([CanBeNull] Action<DbContextOptionsBuilder> optionsAction = null)
            where TContext : DbContext
        {
            _serviceCollection.AddSingleton(p => DbContextOptionsParser.DbContextOptionsFactory<TContext>(p, _configuration, optionsAction));
            _serviceCollection.AddSingleton<DbContextOptions>(p => p.GetRequiredService<DbContextOptions<TContext>>());

            _serviceCollection.AddScoped(typeof(TContext), DbContextActivator.CreateInstance<TContext>);

            return this;
        }

        /// <summary>
        ///     Gets the <see cref="IServiceCollection" /> being configured.
        /// </summary>
        IServiceCollection IAccessor<IServiceCollection>.Service => _serviceCollection;

        /// <summary>
        ///     The configuration for the application, or null if no configuration has been created. This configuration
        ///     may contain Entity Framework specific settings which will be passed into context instances that are
        ///     created by dependency injection.
        /// </summary>
        IConfiguration IAccessor<IConfiguration>.Service => _configuration;
    }
}
