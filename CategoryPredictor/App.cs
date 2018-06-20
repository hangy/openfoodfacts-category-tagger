namespace CategoryPredictor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using CommandDotNet.Attributes;
    using Microsoft.ML;    
    using Microsoft.ML.Data;
    using Microsoft.ML.Trainers;
    using Microsoft.ML.Transforms;
    using Microsoft.ML.Models;
    using Model;

    public class App
    {
        private static readonly string[] separator = new[] { "\t" };

        [ApplicationMetadata(Name = "predict")]
        public async Task TrainModelAsync(string modelPath, string csvPath)
        {
            var model = await PredictionModel.ReadAsync<Product, ProductCategoryPrediction>(modelPath).ConfigureAwait(false);

            using (var s = File.Open(csvPath, FileMode.Open))
            using (var r = new StreamReader(s))
            {
                string line;
                while ((line = await r.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    Predict(model, line);
                }
            }
        }

        private static void Predict(PredictionModel<Product, ProductCategoryPrediction> model, string line)
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
        }
    }
}