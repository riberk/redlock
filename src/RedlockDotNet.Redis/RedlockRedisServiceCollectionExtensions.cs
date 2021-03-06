using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace RedlockDotNet.Redis
{
    /// <summary>Extensions on <see cref="IServiceCollection"/></summary>
    public static class RedlockRedisServiceCollectionExtensions
    {
        private class RedisRedlockBuilder : IRedisRedlockBuilder
        {
            public RedisRedlockBuilder(IServiceCollection services)
            {
                Services = services;
            }

            public IServiceCollection Services { get; }
        }
        
        /// <summary>
        /// Add redis implementation for redlock algorithm
        /// </summary>
        /// <param name="b">Di builder</param>
        /// <param name="build">Build func for construct instances</param>
        /// <returns></returns>
        public static IServiceCollection AddRedisStorage(
            this IRedlockBuilder b, 
            Action<IRedisRedlockBuilder> build
        )
        {
            b.Services.AddOptions();
            b.Services.AddLogging();
            build(new RedisRedlockBuilder(b.Services));
            return b.Services;
        }

        /// <summary>
        /// Add lock instance to di
        /// </summary>
        /// <param name="b"></param>
        /// <param name="connect"><see cref="IConnectionMultiplexer"/> factory</param>
        /// <param name="database">Database number on instance</param>
        /// <param name="name">Instance name (ToString and logs)</param>
        /// <returns></returns>
        public static IRedisRedlockBuilder AddInstance(
            this IRedisRedlockBuilder b,
            Func<IConnectionMultiplexer> connect,
            int database,
            string name
        )
        {
            b.Services.AddSingleton(p =>
            {
                var logger = p.GetRequiredService<ILogger<RedisRedlockInstance>>();
                var key = p.GetRequiredService<IOptions<RedisRedlockOptions>>().Value.RedisKeyFromResourceName;
                var clockDriftFactor = p.GetRequiredService<IOptions<RedlockOptions>>().Value.ClockDriftFactor;
                return RedisRedlockInstance.Create(connect(), key, database, name, clockDriftFactor, logger);
            });
            return b;
        }
        
        /// <summary>
        /// Add lock instance to di
        /// </summary>
        /// <param name="b"></param>
        /// <param name="connect"><see cref="IConnectionMultiplexer"/> factory</param>
        /// <param name="name">Instance name (ToString and logs)</param>
        /// <returns></returns>
        public static IRedisRedlockBuilder AddInstance(this IRedisRedlockBuilder b, Func<IConnectionMultiplexer> connect, string name)
        {
            b.AddInstance(connect, -1, name);
            return b;
        }
        
        /// <summary>
        /// Add lock instance to di
        /// </summary>
        /// <param name="b"></param>
        /// <param name="connect"><see cref="IConnectionMultiplexer"/> factory</param>
        /// <returns></returns>
        public static IRedisRedlockBuilder AddInstance(this IRedisRedlockBuilder b, Func<IConnectionMultiplexer> connect)
        {
            
            b.Services.AddSingleton(p =>
            {
                var logger = p.GetRequiredService<ILogger<RedisRedlockInstance>>();
                var key = p.GetRequiredService<IOptions<RedisRedlockOptions>>().Value.RedisKeyFromResourceName;
                var clockDriftFactor = p.GetRequiredService<IOptions<RedlockOptions>>().Value.ClockDriftFactor;
                return RedisRedlockInstance.Create(connect(), key, clockDriftFactor, logger);
            });
            return b;
        }
        
        /// <summary>
        /// Add lock instance to di
        /// </summary>
        /// <param name="b"></param>
        /// <param name="connection">Connection string for <see cref="ConnectionMultiplexer.Connect(string,System.IO.TextWriter)"/></param>
        /// <param name="database">Database number on instance</param>
        /// <param name="name">Instance name (ToString and logs)</param>
        /// <returns></returns>
        public static IRedisRedlockBuilder AddInstance(this IRedisRedlockBuilder b, string connection, int database, string name) 
            => b.AddInstance(() => ConnectionMultiplexer.Connect(connection), database, name);
        
        /// <summary>
        /// Add lock instance to di
        /// </summary>
        /// <param name="b"></param>
        /// <param name="opt">Options for <see cref="ConnectionMultiplexer.Connect(ConfigurationOptions,System.IO.TextWriter)"/></param>
        /// <param name="database">Database number on instance</param>
        /// <param name="name">Instance name (ToString and logs)</param>
        /// <returns></returns>
        public static IRedisRedlockBuilder AddInstance(this IRedisRedlockBuilder b, ConfigurationOptions opt, int database, string name) 
            => b.AddInstance(() => ConnectionMultiplexer.Connect(opt), database, name);
        
        
        /// <summary>
        /// Add lock instance to di
        /// </summary>
        /// <param name="b"></param>
        /// <param name="connection">Connection string for <see cref="ConnectionMultiplexer.Connect(string,System.IO.TextWriter)"/></param>
        /// <returns></returns>
        public static IRedisRedlockBuilder AddInstance(this IRedisRedlockBuilder b, string connection) 
            => b.AddInstance(() => ConnectionMultiplexer.Connect(connection));
        
        /// <summary>
        /// Add lock instance to di
        /// </summary>
        /// <param name="b"></param>
        /// <param name="opt">Options for <see cref="ConnectionMultiplexer.Connect(ConfigurationOptions,System.IO.TextWriter)"/></param>
        /// <returns></returns>
        public static IRedisRedlockBuilder AddInstance(this IRedisRedlockBuilder b, ConfigurationOptions opt) 
            => b.AddInstance(() => ConnectionMultiplexer.Connect(opt));
        
        
        /// <summary>
        /// Add lock instance to di
        /// </summary>
        /// <param name="b"></param>
        /// <param name="connection">Connection string for <see cref="ConnectionMultiplexer.Connect(string,System.IO.TextWriter)"/></param>
        /// <param name="name">Instance name (ToString and logs)</param>
        /// <returns></returns>
        public static IRedisRedlockBuilder AddInstance(this IRedisRedlockBuilder b, string connection, string name) 
            => b.AddInstance(() => ConnectionMultiplexer.Connect(connection), name);
        
        /// <summary>
        /// Add lock instance to di
        /// </summary>
        /// <param name="b"></param>
        /// <param name="opt">Options for <see cref="ConnectionMultiplexer.Connect(ConfigurationOptions,System.IO.TextWriter)"/></param>
        /// <param name="name">Instance name (ToString and logs)</param>
        /// <returns></returns>
        public static IRedisRedlockBuilder AddInstance(this IRedisRedlockBuilder b, ConfigurationOptions opt, string name) 
            => b.AddInstance(() => ConnectionMultiplexer.Connect(opt), name);

        /// <summary>
        /// Configure <see cref="RedisRedlockOptions"/>
        /// </summary>
        /// <param name="b"></param>
        /// <param name="buildOpt">Configure options</param>
        public static IRedisRedlockBuilder ConfigureOptions(this IRedisRedlockBuilder b, Action<RedisRedlockOptions> buildOpt)
        {
            b.Services.Configure(buildOpt);
            return b;
        }
    }
}