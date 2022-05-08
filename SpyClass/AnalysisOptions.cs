namespace SpyClass
{
    public class AnalysisOptions
    {
        public bool IncludeNonPublicTypes { get; set; } = false;
        public bool IgnoreCompilerGeneratedTypes { get; set; } = true;
    }
}