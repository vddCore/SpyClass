using Mono.Cecil;

namespace SpyClass.DataModel.Documentation.Base
{
    public abstract class DocComponent
    {
        protected ModuleDefinition Module { get; }

        protected DocComponent(ModuleDefinition module)
        {
            Module = module;
        }
    }
}