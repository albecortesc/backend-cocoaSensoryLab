using System;
using Infrastructure.Configuration;
using MongoDB.Driver;

namespace Infrastructure.Services
{
    public sealed class MongoCollectionResolver
    {
        private readonly IMongoDatabase _database;
        private readonly MongoOptions _mongoOptions;

        public MongoCollectionResolver(MongoOptions mongoOptions)
        {
            _mongoOptions = mongoOptions ?? throw new ArgumentNullException(nameof(mongoOptions));
            if (string.IsNullOrWhiteSpace(_mongoOptions.ConnectionString)) throw new InvalidOperationException("Mongo connection string is required.");
            if (string.IsNullOrWhiteSpace(_mongoOptions.DatabaseName)) throw new InvalidOperationException("Mongo database name is required.");

            var client = new MongoClient(_mongoOptions.ConnectionString);
            _database = client.GetDatabase(_mongoOptions.DatabaseName);
        }

        public IMongoCollection<TDocument> GetCollection<TDocument>(string collectionKey)
        {
            var collectionName = _mongoOptions.GetCollectionName(collectionKey);
            return _database.GetCollection<TDocument>(collectionName);
        }
    }
}