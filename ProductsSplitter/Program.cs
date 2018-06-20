namespace ProductsSplitter
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    internal static class Program
    {
        private static readonly string[] columnSeparator = new[] { "\t" };

        private static readonly string[] categorySeparator = new[] { "," };

        private static Task Main(string[] args)
        {
            return Task.WhenAll(args.Select(SplitFileAsync));
        }

        private static async Task SplitFileAsync(string path)
        {
            var classificationFile = Path.ChangeExtension(path, ".class.csv");
            var validationFile = Path.ChangeExtension(path, ".validate.csv");
            var trainingFile = Path.ChangeExtension(path, ".train.csv");
            using (var s = File.Open(path, FileMode.Open))
            using (var r = new StreamReader(s))
            using (var d = File.Open(classificationFile, FileMode.Create))
            using (var w = new StreamWriter(d))
            using (var d2 = File.Open(validationFile, FileMode.Create))
            using (var w2 = new StreamWriter(d2))
            using (var d3 = File.Open(trainingFile, FileMode.Create))
            using (var w3 = new StreamWriter(d3))
            {
                await r.ReadLineAsync().ConfigureAwait(false); // Headers should never be included

                var rand = new Random();

                string line;
                while ((line = await r.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    var split = line.Split(columnSeparator, StringSplitOptions.None);
                    if (split.Length < 15)
                    {
                        continue;
                    }

                    StreamWriter actualWriter;
                    if (!string.IsNullOrWhiteSpace(split[15]))
                    {
                        var inNineteethPercentile = (rand.NextDouble() < 0.90d);
                        actualWriter = inNineteethPercentile ? w3 : w2;
                    }
                    else
                    {
                        actualWriter = w;
                    }

                    await actualWriter.WriteAsync(split[0]).ConfigureAwait(false);
                    await actualWriter.WriteAsync("\t").ConfigureAwait(false);
                    await actualWriter.WriteAsync(split[7]).ConfigureAwait(false);
                    await actualWriter.WriteAsync("\t").ConfigureAwait(false);
                    await actualWriter.WriteAsync(split[8]).ConfigureAwait(false);
                    await actualWriter.WriteAsync("\t").ConfigureAwait(false);
                    await actualWriter.WriteLineAsync(split[15]).ConfigureAwait(false);
                }
            }
        }
    }
}
