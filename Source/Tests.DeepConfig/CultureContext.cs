namespace DeepConfig
{
    using System;
    using System.Globalization;
    using System.Threading;

    public class CultureContext : IDisposable
    {
        private CultureInfo _oldCulture;

        public CultureContext()
            : this(null)
        {
        }

        public CultureContext(CultureInfo c)
        {
            _oldCulture = Thread.CurrentThread.CurrentCulture;

            if (c == null)
            {
                if (_oldCulture.Name.Equals("en-US", StringComparison.OrdinalIgnoreCase))
                {
                    c = CultureInfo.GetCultureInfo("da-DK");
                }
                else
                {
                    c = CultureInfo.GetCultureInfo("en-US");
                }
            }

            Thread.CurrentThread.CurrentCulture = c;
        }

        public void Dispose()
        {
            Thread.CurrentThread.CurrentCulture = _oldCulture;
        }
    }
}
