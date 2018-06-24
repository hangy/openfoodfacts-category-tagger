namespace Model
{
    using System;
    using MongoDB.Bson;
    using MongoDB.Driver;

    public class Vote
    {
        public ObjectId Id { get; set; }

        public MongoDBRef Product { get; set; }

        public MongoDBRef User { get; set; }

        public DateTimeOffset VotedAt { get; set; }

        public Sentiment Sentiment { get; set; }
    }
}