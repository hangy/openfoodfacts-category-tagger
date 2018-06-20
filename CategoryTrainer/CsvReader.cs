namespace CategoryTrainer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using Model;

    public class CsvReader
    {
        private static readonly string[] columnSeparator = new[] { "\t" };

        private static readonly string[] categorySeparator = new[] { "," };

        public IEnumerable<Product> GetData(string path)
        {
            foreach (var line in File.ReadLines(path))
            {
                var split = line.Split(columnSeparator, StringSplitOptions.None);
                if (split.Length < 15 || string.IsNullOrWhiteSpace(split[15]))
                {
                    continue;
                }

                var categories = split[15].Split(categorySeparator, StringSplitOptions.RemoveEmptyEntries);

                for (var i = categories.Length - 1; i >= 0; --i)
                {
                    var product = new Product();
                    product.Code = split[0];
                    product.Name = split[7];
                    product.GenericName = split[8];
                    product.Categories = categories[i];
                    yield return product;
                }
            }
        }
    }
}
