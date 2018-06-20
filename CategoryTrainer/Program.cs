namespace CategoryTrainer
{
    using CommandDotNet;

    internal static class Program
    {
        private static int Main(string[] args)
        {
            AppRunner<App> appRunner = new AppRunner<App>();
            return appRunner.Run(args);
        }        
    }
}
