namespace CategoryPredictor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Authentication;
    using System.Threading.Tasks;
    using CommandDotNet.Attributes;
    using Microsoft.Extensions.Configuration;
    using Microsoft.ML;
    using Microsoft.ML.Data;
    using Microsoft.ML.Trainers;
    using Microsoft.ML.Transforms;
    using Microsoft.ML.Models;
    using MongoDB.Driver;
    using Model;

    public class App
    {
        private static readonly string[] separator = new[] { "\t" };

        private static readonly InsertOneOptions insertOneOptions = new InsertOneOptions();

        public App()
        {
            var devEnvironmentVariable = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            var isDevelopment = string.IsNullOrEmpty(devEnvironmentVariable) || devEnvironmentVariable.ToLower() == "development";

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            if (isDevelopment)
            {
                builder.AddUserSecrets<App>();
            }

            this.Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        [ApplicationMetadata(Name = "predict")]
        public async Task TrainModelAsync(string modelPath, string csvPath)
        {
            var model = await PredictionModel.ReadAsync<Product, ProductCategoryPrediction>(modelPath).ConfigureAwait(false);

            var predictions = new List<ProductPrediction>();
            using (var s = File.Open(csvPath, FileMode.Open))
            using (var r = new StreamReader(s))
            {
                string line;
                while ((line = await r.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    var prediction = Predict(model, line);
                    if (prediction != null)
                    {
                        predictions.Add(prediction);
                    }
                }
            }

            var client = this.CreateMongoClient();
            var database = client.GetDatabase("tagger");
            var collection = database.GetCollection<ProductPrediction>("prediction");

            await collection.InsertManyAsync(predictions).ConfigureAwait(false);
        }

        private static ProductPrediction Predict(PredictionModel<Product, ProductCategoryPrediction> model, string line)
        {
            var split = line.Split(separator, StringSplitOptions.None);

            var p = new Product();
            p.Code = split[0];
            p.Name = split[1];
            p.GenericName = split[2];
            p.Categories = split[3];

            if (string.IsNullOrWhiteSpace(p.Name) || !string.IsNullOrWhiteSpace(p.Categories))
            {
                return null;
            }

            var prediction = model.Predict(p);
            var hasScoreLabelNames = model.TryGetScoreLabelNames(out var scoreLabelNames);
            if (!hasScoreLabelNames)
            {
                return null;
            }

            var scoreForPrediction = GetScoreForLabel(scoreLabelNames, prediction);
            if (!scoreForPrediction.HasValue)
            {
                return null;
            }

            Console.WriteLine("{0}: {1} = {2} ({3:P5})", p.Code, p.Name, prediction.Category, scoreForPrediction);

            if (string.IsNullOrWhiteSpace(prediction.Category))
            {
                return null;
            }

            return new ProductPrediction
            {
                Code = p.Code,
                PredictedCategory = prediction.Category,
                PredictionScore = scoreForPrediction.Value
            };
        }

        private static float? GetScoreForLabel(string[] scoreLabelNames, ProductCategoryPrediction prediction) => GetScoreForLabel(scoreLabelNames, prediction.Score, prediction.Category);

        private static float? GetScoreForLabel(string[] scoreLabelNames, float[] scores, string predictedLabel)
        {
            Debug.Assert(scoreLabelNames.Length == scores.Length);

            for (var index = 0; index < scoreLabelNames.Length; ++index)
            {
                if (scoreLabelNames[index] == predictedLabel)
                {
                    return scores[index];
                }
            }

            return null;
        }

        private MongoClient CreateMongoClient()
        {
            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(this.Configuration["MongoDB"]));
            settings.SslSettings = new SslSettings { EnabledSslProtocols = SslProtocols.Tls12 };
            return new MongoClient(settings);
        }
    }
}