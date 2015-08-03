namespace DeepConfigEditor.ViewModels.ConfigSourceSelection
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Caliburn.Micro;
    using DeepConfig.Providers;
    using DeepConfigEditor.Config;
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Messages;
    using DeepConfigEditor.Resources;
    using DeepConfigEditor.Services;
    using DeepConfigEditor.Utilities;

    public class FileSourceViewModel : PropertyChangedBase, IProvideConfigSource
    {
        private readonly IFileService _fileService;
        private readonly IEventAggregator _messenger;
        private ConfigSourcePurpose _purpose;
        private Action<ConfigSource> _provideSource;
        private FileInfo _selectedFile;

        public FileSourceViewModel(IFileService fileService, IEventAggregator messenger)
        {
            _fileService = fileService;
            _messenger = messenger;
        }

        public Uri IconUri
        {
            get { return As.ResourceUri("File.png"); }
        }
    
        public string DisplayName
        {
            get { return CommonRes.File; }
        }

        public bool CanGetSource
        {
            get;
            private set;
        }

        public string IntroText
        {
            get;
            private set;
        }

        public bool RememberLastLocation
        {
            get;
            set;
        }

        public bool ShowRecentList
        {
            get;
            private set;
        }

        public IEnumerable<ConfigSource> RecentFiles
        {
            get
            {
                return EditorSettings.Instance.RecentSources
                    .Where(s => s.Provider.GetType() == typeof(FileConfigProvider));
            }
        }

        public bool Initialize(ConfigSourcePurpose p, Action<ConfigSource> provideSource)
        {
            _purpose = p;
            _provideSource = provideSource;

            bool isOpenMode = (p == ConfigSourcePurpose.Open);
            this.RememberLastLocation = isOpenMode;
            this.ShowRecentList = (isOpenMode && this.RecentFiles.Any());

            this.IntroText = isOpenMode ? ConfigSourceSelectRes.FileOpenOptionOne : ConfigSourceSelectRes.FileSaveOption;

            return true;
        }

        public void RemoveRecentSource(ConfigSource source)
        {
            EditorSettings.Instance.RecentSources.Remove(source);
            NotifyOfPropertyChange(() => this.RecentFiles);
            _messenger.Publish(new MruListChangedMessage());
        }

        public ConfigSource GetSource()
        {
            return new ConfigSource(new FileConfigProvider(_selectedFile.FullName), this.IconUri);
        }

        public void ResetLocation()
        {
            EditorSettings.Instance.FileSourceSelect.LastDirectory = null;
        }

        public void Browse()
        {
            if (_purpose == ConfigSourcePurpose.Open)
            {
                _selectedFile = BrowseToOpen();
            }
            else
            {
                _selectedFile = BrowseToSave();
            }

            if (_selectedFile == null)
            {
                return;
            }

            if (this.RememberLastLocation)
            {
                EditorSettings.Instance.FileSourceSelect.LastDirectory = _selectedFile.DirectoryName;
            }

            //We always provide the source directly instead of notifying that its available, no need for the user to click two separate ok buttons to make a selection
            var s = GetSource();
            _provideSource(s);
        }

        private FileInfo BrowseToOpen()
        {
            var dialog = _fileService.OpenService;
            dialog.Filter = ConfigSourceSelectRes.FileSelectFilter;
            dialog.InitialDirectory = GetInitialDirectory();
            dialog.Multiselect = false;
            dialog.Title = ConfigSourceSelectRes.FileSelectTitle;

            if (dialog.DetermineFile())
            {
                return dialog.File;
            }

            return null;
        }

        private FileInfo BrowseToSave()
        {
            var dialog = _fileService.SaveService;
            dialog.Filter = ConfigSourceSelectRes.FileSelectFilter;
            dialog.InitialDirectory = GetInitialDirectory();
            dialog.DefaultExt = ".config";
            dialog.Title = ConfigSourceSelectRes.FileSaveTitle;

            if (dialog.DetermineFile())
            {
                return dialog.File;
            }

            return null;
        }

        private string GetInitialDirectory()
        {
            var dir = EditorSettings.Instance.FileSourceSelect.LastDirectory;

            if (string.IsNullOrEmpty(dir))
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }

            if (!Directory.Exists(dir))
            {
                ResetLocation();

                return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }

            return dir;
        }
    }
}
