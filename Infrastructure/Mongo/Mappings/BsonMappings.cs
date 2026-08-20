namespace Infrastructure.Mongo.Mappings
{
    public static class BsonMappings
    {
        public static void Register()
        {
            UserBsonMapping.Register();
        }
    }
}