namespace DeepConfigEditor
{
    using System;
    using System.Reflection;

    public static class ResourceHelper
    {
        public static Uri ResourceUri(string resource, string subfolder = "Icons", Assembly assembly = null)
        {
            assembly = assembly ?? typeof(ResourceHelper).Assembly;

            return new Uri(string.Format(
                "pack://application:,,,/{0};component/{1}/{2}",
                assembly.GetAssemblyName(),
                subfolder,
                resource));
        }

        private static string GetAssemblyName(this Assembly assembly)
        {
            return assembly.FullName.Remove(assembly.FullName.IndexOf(','));
        }
    }
}
