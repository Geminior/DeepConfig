namespace DeepConfigEditor.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Caliburn.Micro;
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Resources;

    public class OptionsViewModel : Conductor<IEditorOptions>.Collection.OneActive, ISupplyStatusInfo
    {
        private readonly IEventAggregator _messenger;
        private readonly IEnumerable<IEditorOptions> _optionPages;

        public OptionsViewModel(IEnumerable<IEditorOptions> optionPages, IEventAggregator messenger)
        {
            _messenger = messenger;
            _optionPages = optionPages;
        }

        public string StatusMessage
        {
            get { return OptionsRes.OptionsStatusMessage; }
        }

        public string StatusState
        {
            get { return string.Empty; }
        }

        public void Cancel()
        {
            TryClose();
        }

        protected override void OnInitialize()
        {
            this.Items.AddRange(_optionPages);
        }
    }
}
