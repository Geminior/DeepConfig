namespace DeepConfig.TestTypes
{
    using System;

    public class DeepConfigTestException : Exception
    {
        public DeepConfigTestException()
            : base()
        {
        }

        public DeepConfigTestException(string message)
            : base(message)
        {
        }
    }
}
