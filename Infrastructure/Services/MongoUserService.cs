using System;
using System.Collections.Generic;
using Application.Interfaces;
using Infrastructure.Mongo.Mappings;
using MongoDB.Bson;
using MongoDB.Driver;
using UserEntity = Domain.Entities.User;

namespace Infrastructure.Services
{
    public class MongoUserService : IUserService
    {    
        private readonly IMongoCollection<UserEntity> _collection;      

        public MongoUserService(MongoCollectionResolver collectionResolver, string collectionKey = "Users")
        {
            if (collectionResolver == null) throw new ArgumentNullException(nameof(collectionResolver));

            BsonMappings.Register();
            _collection = collectionResolver.GetCollection<UserEntity>(collectionKey);
        }

        public MongoUserService(string connectionString, string databaseName, string collectionName)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new InvalidOperationException("Mongo connection string is required.");
            if (string.IsNullOrWhiteSpace(databaseName)) throw new InvalidOperationException("Mongo database name is required.");
            if (string.IsNullOrWhiteSpace(collectionName)) throw new InvalidOperationException("Mongo collection name is required.");

            BsonMappings.Register();
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _collection = database.GetCollection<UserEntity>(collectionName);
        }

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static string NormalizeEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return string.Empty;
            return email.Trim().ToLowerInvariant();
        }

        public async System.Threading.Tasks.Task UserRegister(UserEntity user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            user.Email = NormalizeEmail(user.Email);

            var existing = await _collection.Find(u => u.Email == user.Email).FirstOrDefaultAsync();
            if (existing != null) throw new InvalidOperationException("A user with that email already exists.");

            var existingCedula = await _collection.Find(u => u.Cedula == user.Cedula).FirstOrDefaultAsync();
            if (existingCedula != null) throw new InvalidOperationException("A user with that cedula already exists.");

            // Ensure the user has an Id compatible with the BSON mapping (string of ObjectId)
            if (string.IsNullOrWhiteSpace(user.Id)) user.Id = ObjectId.GenerateNewId().ToString();

            await _collection.InsertOneAsync(user);
        }

        public async System.Threading.Tasks.Task<bool> UserAuthenticate(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return false;

            var emailNormalized = NormalizeEmail(email);
            var user = await _collection.Find(u => u.Email == emailNormalized).FirstOrDefaultAsync();
            if (user == null) return false;

            return user.VerifyPassword(password);
        }

        public async System.Threading.Tasks.Task<IEnumerable<UserEntity>> UserGetAll()
        {
            return await _collection.Find(FilterDefinition<UserEntity>.Empty).ToListAsync();
        }

        public async System.Threading.Tasks.Task UserUpdateByCedulaOrEmail(long? cedula, string? email, UserEntity updatedUser)
        {
            if (updatedUser == null) throw new ArgumentNullException(nameof(updatedUser));

            var hasCedula = cedula.HasValue;
            var emailNormalized = NormalizeEmail(email);
            if (!hasCedula && string.IsNullOrWhiteSpace(emailNormalized))
                throw new ArgumentException("Cedula or email is required to identify the user.", nameof(cedula));

            var filter = hasCedula
                ? Builders<UserEntity>.Filter.Eq(u => u.Cedula, cedula!.Value)
                : Builders<UserEntity>.Filter.Eq(u => u.Email, emailNormalized);

            var existing = await _collection.Find(filter).FirstOrDefaultAsync();
            if (existing == null) throw new InvalidOperationException("User not found.");

            updatedUser.Email = NormalizeEmail(updatedUser.Email);

            var existingCedula = await _collection.Find(u => u.Cedula == updatedUser.Cedula && u.Id != existing.Id)
                .FirstOrDefaultAsync();
            if (existingCedula != null) throw new InvalidOperationException("A user with that cedula already exists.");

            var existingEmail = await _collection.Find(u => u.Email == updatedUser.Email && u.Id != existing.Id)
                .FirstOrDefaultAsync();
            if (existingEmail != null) throw new InvalidOperationException("A user with that email already exists.");

            updatedUser.Id = existing.Id;
            await _collection.ReplaceOneAsync(u => u.Id == existing.Id, updatedUser);
        }

        public async System.Threading.Tasks.Task<UserEntity?> UserFindByCedulaOrEmail(long? cedula, string? email)
        {
            var hasCedula = cedula.HasValue;
            var emailNormalized = NormalizeEmail(email);
            if (!hasCedula && string.IsNullOrWhiteSpace(emailNormalized))
                throw new ArgumentException("Cedula or email is required to identify the user.", nameof(cedula));

            var filter = hasCedula
                ? Builders<UserEntity>.Filter.Eq(u => u.Cedula, cedula!.Value)
                : Builders<UserEntity>.Filter.Eq(u => u.Email, emailNormalized);

            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async System.Threading.Tasks.Task UserDeleteByCedulaOrEmail(long? cedula, string? email)
        {
            var hasCedula = cedula.HasValue;
            var emailNormalized = NormalizeEmail(email);
            if (!hasCedula && string.IsNullOrWhiteSpace(emailNormalized))
                throw new ArgumentException("Cedula or email is required to identify the user.", nameof(cedula));

            var filter = hasCedula
                ? Builders<UserEntity>.Filter.Eq(u => u.Cedula, cedula!.Value)
                : Builders<UserEntity>.Filter.Eq(u => u.Email, emailNormalized);

            var result = await _collection.DeleteOneAsync(filter);
            if (result.DeletedCount == 0) throw new InvalidOperationException("User not found.");
        }
    }
}
