namespace Model
{
    using Microsoft.ML.Runtime.Api;

    public class Product
    {
        [Column(ordinal: "0")]
        public string Code;

        [Column(ordinal: "1")]
        public string Name;

        [Column(ordinal: "2")]
        public string GenericName;

        [Column(ordinal: "3")]
        public string Categories;
    }
}
