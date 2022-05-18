namespace SpyClass.Analysis
{
    public class AnalysisOptions
    {
        public bool IncludeNonPublicTypes { get; set; } = false;
        public bool IncludeNonUserMembers { get; set; } = false;
        public bool IgnoreCompilerGeneratedTypes { get; set; } = true;
    }
}