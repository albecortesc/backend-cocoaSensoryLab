using System;
using System.Collections.Generic;

namespace Infrastructure.Configuration
{
    /// <summary>
    /// Configuration options for MongoDB.
    /// This class is designed to be bound to a configuration section (e.g., "Mongo") in appsettings.json or environment variables.
    /// It includes properties for the connection string, database name, and a dictionary of collection names
    /// that can be accessed by a key. The GetCollectionName method provides a way to retrieve collection names safely, throwing exceptions if the key is missing or invalid.
    /// </summary>
    /// <remarks>
    /// Example configuration in appsettings.json:
    /// {
    ///   "Mongo": {    
    ///   ///     "ConnectionString": "mongodb://localhost:27017",
    ///   ///     "DatabaseName": "MyDatabase",
    ///   ///     "Collections": {
    ///   ///   ///         "Users": "users",
    ///   ///     }
    ///   /// }
    /// </remarks>
    public sealed class MongoOptions
    {
        public string ConnectionString { get; init; } = "mongodb://localhost:27017";
        public string DatabaseName { get; init; } = "cocoa_sensory_lab_db";
        public Dictionary<string, string> Collections { get; init; } = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Users"] = "users",
            ["Tasks"] = "tasks"
        };

        public string GetCollectionName(string collectionKey)
        {
            if (string.IsNullOrWhiteSpace(collectionKey))
                throw new ArgumentException("Collection key is required.", nameof(collectionKey));

            if (!Collections.TryGetValue(collectionKey, out var collectionName) || string.IsNullOrWhiteSpace(collectionName))
                throw new InvalidOperationException($"Mongo collection key '{collectionKey}' is not configured. Use Mongo:Collections:{collectionKey}.");

            return collectionName;
        }
    }
}