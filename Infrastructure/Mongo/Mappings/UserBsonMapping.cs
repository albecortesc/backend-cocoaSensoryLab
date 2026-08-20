using Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;

namespace Infrastructure.Mongo.Mappings
{
    internal static class UserBsonMapping
    {
        public static void Register()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(User)))
            {
                BsonClassMap.RegisterClassMap<User>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);
                    cm.MapIdMember(c => c.Id)
                      .SetIdGenerator(StringObjectIdGenerator.Instance)
                      .SetSerializer(new StringSerializer(BsonType.ObjectId));
                });
            }
        }
    }
}