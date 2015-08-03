namespace DeepConfig.Providers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using DeepConfig.Utilities;

    /// <summary>
    /// Implementation of <see cref="IConfigProvider"/> that reads and stores settings from the currently logged in windows user's appdata store.
    /// </summary>
    /// <remarks>
    /// The default file(s) can either be the application config file (web.config or applicationname.exe.config) or one or more specific files.
    /// <para>
    /// Settings are read from the user specific file, but any settings not present there will be set to those of the specified file(s).
    /// Saving a configuration will only save to the user specific file, the default settings file(s) provided is never changed.
    /// </para>
    /// </remarks>
    public sealed class UserFileConfigProvider : MultiFileConfigProvider
    {
        private string _userFilePath;
        private UserStore _location;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserFileConfigProvider"/> class.
        /// </summary>
        /// <param name="location">The target user storage location</param>
        public UserFileConfigProvider(UserStore location)
            : this(location, AppDomain.CurrentDomain.SetupInformation.ConfigurationFile)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserFileConfigProvider"/> class.
        /// </summary>
        /// <param name="location">The target user storage location</param>
        /// <param name="configFilePaths">The list of default config files (file path) to load inorder of precedence.</param>
        public UserFileConfigProvider(UserStore location, params string[] configFilePaths)
            : this(location, (IEnumerable<string>)configFilePaths)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserFileConfigProvider"/> class.
        /// </summary>
        /// <param name="location">The target user storage location</param>
        /// <param name="configFilePaths">The list of default config files (file path) to load in order of precedence.</param>
        public UserFileConfigProvider(UserStore location, IEnumerable<string> configFilePaths)
            : base(configFilePaths)
        {
            //TODO:Finish implemenation of IsolatedStorage option
            _location = location;

            ResolveUserStore();
        }

        /// <summary>
        /// The storage location for storing the user specific config settings
        /// </summary>
        public enum UserStore
        {
            /// <summary>
            /// The user's appdata roaming profile
            /// </summary>
            Roaming,

            /// <summary>
            /// The user's appdata local profile
            /// </summary>
            Local,

            /// <summary>
            /// Isolated storage
            /// </summary>
            Isolated
        }

        /// <summary>
        /// Gets a value indicating whether the configuration is read-only, i.e. the file is read-only.
        /// </summary>
        public override bool IsReadOnly
        {
            get
            {
                if (_userFilePath != null && File.Exists(_userFilePath))
                {
                    FileAttributes fileAttribs = File.GetAttributes(_userFilePath);
                    return ((fileAttribs & FileAttributes.ReadOnly) == FileAttributes.ReadOnly);
                }

                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the configuration file can be deleted.
        /// </summary>
        public override bool CanDelete
        {
            get { return !IsReadOnly; }
        }

        /// <summary>
        /// Save the configuration back to disk
        /// </summary>
        /// <param name="configDoc">The configuration xml to save.</param>
        /// <exception cref="System.IO.IOException">If <see cref="ConfigFileName"/> cannot be written to for some reason. File can be non-existant though this will just create the file.</exception>
        /// <exception cref="UnauthorizedAccessException">If the file is read-only or otherwise inaccessible.</exception>
        public override void SaveConfig(XDocument configDoc)
        {
            if (configDoc == null)
            {
                return;
            }

            string folder = Path.GetDirectoryName(_userFilePath);
            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            configDoc.Save(_userFilePath);

            this.LoadTime = File.GetLastWriteTimeUtc(_userFilePath);
        }

        /// <summary>
        /// Deletes the configuration file
        /// </summary>
        public override void DeleteConfig()
        {
            if (!this.CanDelete)
            {
                throw new InvalidOperationException("The configuration cannot be deleted, please check the CanDelete property before attempting to delete a configuration.");
            }

            if (File.Exists(_userFilePath))
            {
                File.Delete(_userFilePath);
            }
        }

        private void ResolveUserStore()
        {
            if (_location != UserStore.Isolated)
            {
                var folder = _location == UserStore.Roaming ? Environment.SpecialFolder.ApplicationData : Environment.SpecialFolder.LocalApplicationData;

                try
                {
                    _userFilePath = Path.Combine(
                        ConfigHelper.ResolveStorageLocation(folder, true),
                        "user.config");
                }
                catch (Exception e)
                {
                    throw new ConfigException("Failed to resolve user store location.", ConfigErrorCode.FailedToLoad, e);
                }
            }
            else
            {
                _userFilePath = "user.config";
            }

            this.ConfigFiles.Insert(0, _userFilePath);
        }
    }
}
