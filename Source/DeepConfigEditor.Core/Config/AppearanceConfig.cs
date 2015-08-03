namespace DeepConfigEditor.Config
{
    using DeepConfig;

    public sealed class AppearanceConfig : AutoSingletonConfig<AppearanceConfig>
    {
        public AppearanceConfig()
        {
            this.Width = 610.0;
            this.Height = 410.0;
        }

        public enum StartState
        {
            Normal,
            Maximized,
            Minimized
        }

        [ConfigSetting(Description = "Top position of the Editor Window.")]
        public double? Top { get; set; }

        [ConfigSetting(Description = "Left position of the Editor Window.")]
        public double? Left { get; set; }

        [ConfigSetting(Description = "Width of the Editor Window.")]
        public double Width { get; set; }

        [ConfigSetting(Description = "Height of the Editor Window.")]
        public double Height { get; set; }

        [ConfigSetting(Description = "What state to start the Editor window in.")]
        public StartState WindowStartState { get; set; }

        [ConfigSetting(Description = "Whether to keep the Editor Window as the topmost window always.")]
        public bool AlwaysOnTop { get; set; }
    }
}
