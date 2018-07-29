namespace Model
{
    using Microsoft.ML.Runtime.Api;

    public class ProductCategoryPrediction
    {
        [ColumnName("PredictedLabel")]
        public string Category;

        [ColumnName("Score")]
        public float[] Score;
    }
}
