namespace SPA.Messaging
{
    using MediatR;
    using Model;
    using MongoDB.Bson;

    public class AddVoteRequest : IRequest<ObjectId>
    {
        public string Code { get; set; }

        public ObjectId UserId { get; set; }

        public Sentiment Sentiment { get; set; }
    }
}
