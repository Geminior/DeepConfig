namespace DeepConfig.TestTypes
{
    using System.Collections.Generic;

    public class ListWithNoDefaultCtor : List<string>
    {
        public ListWithNoDefaultCtor(string someVal)
            : base()
        {
        }
    }
}
