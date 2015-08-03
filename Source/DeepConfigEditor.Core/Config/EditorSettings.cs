namespace DeepConfigEditor.Config
{
    using System.Collections.Generic;
    using Caliburn.Micro;
    using DeepConfig;
    using DeepConfig.Utilities;
    using DeepConfigEditor.Extensions;

    public class EditorSettings : AutoSingletonConfig<EditorSettings>
    {
        private readonly BindableCollection<ConfigSource> _recentSources = new BindableCollection<ConfigSource>();
        private FileSourceConfig _fileSourceSelect = new FileSourceConfig();
        private int _recentInMenu = 10;
        private int _recentTotal = 50;

        [ConfigSetting]
        public FileSourceConfig FileSourceSelect
        {
            get { return _fileSourceSelect; }
        }

        [ConfigSetting]
        public IList<ConfigSource> RecentSources
        {
            get
            {
                return _recentSources;
            }
        }

        [ConfigSetting]
        public int RecentInMenu
        {
            get
            {
                return _recentInMenu;
            }

            set
            {
                if (value > 10)
                {
                    value = 10;
                }

                _recentInMenu = value;
            }
        }

        [ConfigSetting]
        public int RecentTotal
        {
            get
            {
                return _recentTotal;
            }

            set
            {
                if (value > 50)
                {
                    value = 50;
                }

                _recentTotal = value;
            }
        }

        public void AddRecentSource(ConfigSource s)
        {
            //Do not add a source if its provider cannot be persisted to file
            if (s.Provider.GetType().GetAttribute<ConfigSectionAttribute>(true) == null)
            {
                return;
            }

            lock (_recentSources)
            {
                RemoveSourceNoLock(s);

                int trimCount = _recentSources.Count - _recentTotal;
                for (int i = -1; i < trimCount; i++)
                {
                    _recentSources.RemoveAt(_recentSources.Count - 1);
                }

                _recentSources.Insert(0, s);
            }
        }

        public void RemoveRecentSource(ConfigSource s)
        {
            lock (_recentSources)
            {
                RemoveSourceNoLock(s);
            }
        }

        private void RemoveSourceNoLock(ConfigSource s)
        {
            for (int i = _recentSources.Count - 1; i >= 0; i--)
            {
                var item = _recentSources[i];
                if (item.Provider.SourceIdentifier == s.Provider.SourceIdentifier)
                {
                    _recentSources.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
