namespace DeepConfigEditor.ViewModels.Options
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Caliburn.Micro;
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Resources;

    public class PluginsViewModel : PropertyChangedBase, IEditorOptions
    {
        public PluginsViewModel()
        {
            this.DisplayName = OptionsRes.Plugins;
        }

        public string DisplayName
        {
            get;
            set;
        }
    }
}
