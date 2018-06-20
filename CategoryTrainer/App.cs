namespace CategoryTrainer
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
        [ApplicationMetadata(Name = "train")]
        public async Task TrainModelAsync(string csvPath, string modelPath)
        {
            var pipeline = new LearningPipeline();

            pipeline.Add(CollectionDataSource.Create(new CsvReader().GetData(csvPath)));

            pipeline.Add(new Dictionarizer(("Categories", "Label")));

            pipeline.Add(new TextFeaturizer("Name", "Name"));

            pipeline.Add(new TextFeaturizer("GenericName", "GenericName"));

            pipeline.Add(new ColumnConcatenator("Features", "Name", "GenericName"));

            pipeline.Add(new StochasticDualCoordinateAscentClassifier() { NumThreads = Math.Max(2, Environment.ProcessorCount - 1) });
            pipeline.Add(new PredictedLabelColumnOriginalValueConverter() { PredictedLabelColumn = "PredictedLabel" });

            Console.WriteLine("=============== Training model ===============");

            var model = pipeline.Train<Product, ProductCategoryPrediction>();

            await model.WriteAsync(modelPath).ConfigureAwait(false);

            Console.WriteLine("=============== End training ===============");
            Console.WriteLine("The model is saved to {0}", modelPath);
        }

        [ApplicationMetadata(Name = "evaluate")]
        public async Task EvaluateAsync(string modelPath, string csvPath)
        {
            var model = await PredictionModel.ReadAsync<Product, ProductCategoryPrediction>(modelPath).ConfigureAwait(false);

            // To evaluate how good the model predicts values, the model is ran against new set
            // of data (test data) that was not involved in training.
            var testData = new TextLoader(csvPath).CreateFrom<Product>(useHeader: false, allowQuotedStrings: false, supportSparse: false);

            // ClassificationEvaluator performs evaluation for Multiclass Classification type of ML problems.
            var evaluator = new ClassificationEvaluator { OutputTopKAcc = 3 };

            Console.WriteLine("=============== Evaluating model ===============");

            var metrics = evaluator.Evaluate(model, testData);
            Console.WriteLine("Metrics:");
            Console.WriteLine($"    AccuracyMacro = {metrics.AccuracyMacro:0.####}, a value between 0 and 1, the closer to 1, the better");
            Console.WriteLine($"    AccuracyMicro = {metrics.AccuracyMicro:0.####}, a value between 0 and 1, the closer to 1, the better");
            Console.WriteLine($"    LogLoss = {metrics.LogLoss:0.####}, the closer to 0, the better");
            Console.WriteLine($"    LogLoss for class 1 = {metrics.PerClassLogLoss[0]:0.####}, the closer to 0, the better");
            Console.WriteLine($"    LogLoss for class 2 = {metrics.PerClassLogLoss[1]:0.####}, the closer to 0, the better");
            Console.WriteLine($"    LogLoss for class 3 = {metrics.PerClassLogLoss[2]:0.####}, the closer to 0, the better");

            Console.WriteLine("=============== End evaluating ===============");
            Console.WriteLine();
        }
    }
}