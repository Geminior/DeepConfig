namespace DeepConfigEditor.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Caliburn.Micro;
    using Caliburn.Micro.Extras;
    using DeepConfig.Providers;
    using DeepConfigEditor.Services;

    public class ConfigurationWrapper : PropertyChangedBase
    {
        private string _lastDummy;

        public string DummyContents
        {
            get;
            set;
        }

        public bool HasChanges
        {
            get { return _lastDummy != this.DummyContents; }
        }

        public void Load(IConfigProvider provider)
        {
            //throw new Exception("Failed to load config, no documents returned.");
            Thread.Sleep(2000);
            //throw new Exception("Failed to load config, no documents returned.");

            //TODO: this is all mock up stuff
            var doc = provider.LoadConfig().FirstOrDefault();

            if (doc == null)
            {
                throw new Exception("Failed to load config, no documents returned.");
            }

            this.DummyContents = _lastDummy = doc.ToString();
            NotifyOfPropertyChange(() => this.DummyContents); 
        }

        public void Save(IConfigProvider provider)
        {
            Thread.Sleep(5000);

            var doc = XDocument.Parse(this.DummyContents);
            provider.SaveConfig(doc);

            _lastDummy = DummyContents;
        }

        public void Delete(IConfigProvider provider)
        {
            if (!provider.CanDelete)
            {
                return;
            }

            Thread.Sleep(4000);

            //Make sure haschanges returns false
            _lastDummy = DummyContents;

            provider.DeleteConfig();
        }
    }
}
