namespace CategoryPredictor
{
    using System;
    using System.Collections.Generic;
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

            var client = this.CreateMongoClient();
            var database = client.GetDatabase("tagger");
            var collection = database.GetCollection<ProductPrediction>("prediction");

            using (var s = File.Open(csvPath, FileMode.Open))
            using (var r = new StreamReader(s))
            {
                string line;
                while ((line = await r.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    await PredictAsync(model, collection, line);
                }
            }
        }

        private static async Task PredictAsync(PredictionModel<Product, ProductCategoryPrediction> model, IMongoCollection<ProductPrediction> collection, string line)
        {
            var split = line.Split(separator, StringSplitOptions.None);

            var p = new Product();
            p.Code = split[0];
            p.Name = split[1];
            p.GenericName = split[2];
            p.Categories = split[3];

            if (string.IsNullOrWhiteSpace(p.Name) || !string.IsNullOrWhiteSpace(p.Categories))
            {
                return;
            }
            
            var prediction = model.Predict(p);
            Console.WriteLine("{0}: {1} = {2}", p.Code, p.Name, prediction.Category);

            if (!string.IsNullOrWhiteSpace(prediction.Category))
            {
                await collection.InsertOneAsync(new ProductPrediction{
                    Code = p.Code,
                    PredictedCategory = prediction.Category
                });
            }
        }

        private MongoClient CreateMongoClient()
        {
            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(this.Configuration["MongoDB"]));
            settings.SslSettings = new SslSettings { EnabledSslProtocols = SslProtocols.Tls12 };
            return new MongoClient(settings);
        }
    }
}