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

        protected string StringifyConstant(object constant)
        {
            if (constant == null)
            {
                return "null";
            }
            else if (constant.GetType().FullName == typeof(string).FullName)
            {
                return "\"" + constant + "\"";
            }
            else
            {
                return constant.ToString();
            }
        }
    }
}