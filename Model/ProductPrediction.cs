namespace Model
{
    using System.Collections.Generic;
    using MongoDB.Bson;

    public class ProductPrediction
    {
        public ObjectId Id { get; set; }

        public string Code { get; set; }

        public string PredictedCategory { get; set; }
    }
}