namespace SPA.Messaging
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Model;
    using MongoDB.Bson;
    using MongoDB.Driver;

    public class AddVoteRequestHandler : IRequestHandler<AddVoteRequest, ObjectId>
    {
        private readonly IMongoDatabase db;

        public AddVoteRequestHandler(IMongoDatabase db)
        {
            this.db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<ObjectId> Handle(AddVoteRequest request, CancellationToken cancellationToken)
        {
            var predictions = this.db.GetCollection<ProductPrediction>("prediction");
            var prediction = await predictions.Find(_ => _.Code == request.Code).SingleAsync().ConfigureAwait(false);
            if (prediction == null)
            {
                return default(ObjectId);
            }

            var votes = this.db.GetCollection<Vote>("vote");
            var vote = new Vote
            {
                Product = new MongoDBRef("prediction", prediction.Id),
                Sentiment = request.Sentiment
            };
            await votes.InsertOneAsync(vote, cancellationToken).ConfigureAwait(false);
            return vote.Id;
        }
    }
}
