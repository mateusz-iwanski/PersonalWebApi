namespace PersonalWebApi.Processes.NopCommerce.Events
{
    public class ProductNopEvents
    {
        // start paraphrase process
        public const string StartProcess = nameof(StartProcess);

        public const string ReadedProduct = nameof(ReadedProduct);
        public const string ReadedCategory = nameof(ReadedCategory);

        // end
        public const string Paraphrased = nameof(Paraphrased);
    }
}
