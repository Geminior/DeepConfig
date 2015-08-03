namespace DeepConfigEditor.Utilities
{
    public static class Is
    {
        public static bool Type<T>(object instance)
        {
            if (instance == null)
            {
                return false;
            }

            return instance.GetType() == typeof(T);
        }
    }
}
