namespace Model
{
    using System.Collections.Generic;

    public class ProductPrediction
    {
        public string Code { get; set; }

        public string PredictedCategory { get; set; }

        public List<Vote> Votes { get; set; }
    }
}