namespace SpyClass.Analysis.DataModel.Documentation.Base
{
    public abstract class DocComponent
    {
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