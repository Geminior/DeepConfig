namespace DeepConfig.TestTypes
{
    public class NestedSingletonUse : SingletonConfig<NestedSingletonUse>
    {
        public string Name
        {
            get
            {
                return SingletonType.Instance.Name;
            }
        }
    }
}
