using MachineStreamCore.Configuration;
using MachineStreamCore.Interfaces;
using MachineStreamCore.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace MachineStreamCore.Services
{
    /// <summary>
    /// Static class used to get the instance of the MongoClient needed for the DI registration
    /// </summary>
    public static class MongoSettings
    {
        /// <summary>
        /// It create an istance of the mongo client using the ClientSettingsConfiguration passed
        /// </summary>
        /// <param name="clientSettingsConfiguration">POCO class contains the params for the mongo connection</param>
        /// <returns></returns>
        public static IMongoClient GetMongoClient(ClientSettingsConfiguration clientSettingsConfiguration)
        {
            return new MongoClient(GetMongoClientSettings(clientSettingsConfiguration));
        }

        private static MongoClientSettings GetMongoClientSettings(ClientSettingsConfiguration clientSettingsConfiguration)
        {
            MongoClientSettings settings = new MongoClientSettings
            {
                Server = new MongoServerAddress(clientSettingsConfiguration.Server, clientSettingsConfiguration.Port)
            };

            if (clientSettingsConfiguration.UseSSL)
            {
                settings.UseSsl = true;
                settings.SslSettings = new SslSettings();
                settings.SslSettings.EnabledSslProtocols = SslProtocols.Tls12;
            }

            MongoIdentity identity = new MongoInternalIdentity(clientSettingsConfiguration.AuthenticationDB, clientSettingsConfiguration.UserName);
            MongoIdentityEvidence evidence = new PasswordEvidence(clientSettingsConfiguration.Password);

            settings.Credential = new MongoCredential(clientSettingsConfiguration.Mechanism, identity, evidence);

            return settings;
        }
    }

    /// <summary>
    /// Class that implement the exposed methods for the IMongo interface using the MongoDB.Driver
    /// </summary>
    public class MongoService : IMongo
    {
        private readonly ILogger logger;
        private readonly MongoDBConfiguration mongoDBConfiguration;
        private readonly IMongoClient mongoClient;

        /// <summary>
        /// Constructor used by the DI
        /// </summary>
        /// <param name="mongoClient">The injected interface of the mongo client</param>
        /// <param name="optionsMongoDB">The IOptions that contains the mongo configurations</param>
        /// <param name="logger">The injected logger</param>
        public MongoService(IMongoClient mongoClient, IOptions<MongoDBConfiguration> optionsMongoDB, ILogger<MongoService> logger)
        {
            this.logger = logger;
            mongoDBConfiguration = optionsMongoDB.Value;
            this.mongoClient = mongoClient;
        }

        /// <summary>
        /// Method that contains the business logic to insert a single event to Mongo using the MongoDB.Driver
        /// </summary>
        /// <param name="webSocketEvent">The WebSocketEvent we need to insert to mongo</param>
        /// <returns></returns>
        public async Task InsertEventAsync(WebSocketStream webSocketEvent)
        {
            logger.LogDebug($"InsertEventAsync({webSocketEvent}) using MongoService");
            try
            {
                await GetEventsCollection().InsertOneAsync(webSocketEvent);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"InsertEventAsync({webSocketEvent}) using MongoService caused error");
            }
        }

        /// <summary>
        /// Perform a find on the events collection based on the params passed
        /// </summary>
        /// <param name="filters">The optional filters to apply, if null apply emply filter</param>
        /// <param name="sort">The optional sort to apply, if null no sorting applied</param>
        /// <param name="limit">The limit of the records to fetch from the DB</param>
        /// <returns></returns>
        public async Task<List<WebSocketStream>> FindEventsByFiltersAsync(FilterDefinition<WebSocketStream> filters = null, SortDefinition<WebSocketStream> sort = null, int limit = 100)
        {
            logger.LogDebug($"FindEventsByFiltersAsync({filters}, {sort ?? "N.A."}, {limit}) using MongoService");
            try
            {
                if (filters == null)
                    filters = Builders<WebSocketStream>.Filter.Empty;
                if (sort == null)
                    return await GetEventsCollection().Find(filters).Limit(limit).ToListAsync();
                return await GetEventsCollection().Find(filters).Sort(sort).Limit(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"FindEventsByFiltersAsync({filters}, {sort ?? "N.A."}, {limit}) using MongoService caused error");
                throw ex;
            }
        }

        private IMongoCollection<WebSocketStream> GetEventsCollection()
        {
            return GetMongoDatabase(mongoDBConfiguration.DatabaseName)
                .GetCollection<WebSocketStream>(mongoDBConfiguration.EventCollection);
        }

        private IMongoDatabase GetMongoDatabase(string databaseName)
        {
            return mongoClient.GetDatabase(databaseName);
        }
    }
}
