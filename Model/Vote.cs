namespace Model
{
    using System;

    public class Vote
    {
        public Guid VoterId { get; set; }

        public DateTimeOffset VotedAt { get; set; }

        public Sentiment Sentiment { get; set; }
    }
}