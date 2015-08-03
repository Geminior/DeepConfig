namespace DeepConfigEditor.ViewModels.Options
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Caliburn.Micro;
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Resources;

    public class GeneralViewModel : PropertyChangedBase, IEditorOptions, IDataErrorInfo
    {
        public GeneralViewModel()
        {
            this.DisplayName = OptionsRes.Settings;
        }

        public string DisplayName
        {
            get;
            set;
        }

        public int RecentInMenu
        {
            get;
            set;
        }

        public int RecentTotal
        {
            get;
            set;
        }

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get { throw new NotImplementedException(); }
        }
    }
}
