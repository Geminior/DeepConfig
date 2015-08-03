namespace DeepConfigEditor.Models
{
    using DeepConfigEditor.Extensions;

    internal sealed class ConfigInfo : IConfigInfo
    {
        public bool Exists { get; set; }

        public bool IsReadOnly { get; set; }

        public bool IsNew { get; set; }

        public bool CanDelete { get; set; }

        public string FriendlyName { get; set; }
    }
}
