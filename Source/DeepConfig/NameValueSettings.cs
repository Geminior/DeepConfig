namespace DeepConfig
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Xml;
    using DeepConfig.Handlers;
    using DeepConfig.Utilities;

    /// <summary>
    /// Encapsulates basic name value config sections. This class is ineffecient and should only be used while transitioning to concrete config types.
    /// </summary>
    [Serializable]
    [ConfigSection(typeof(NameValueExtendedSectionHandler))]
    public sealed class NameValueSettings
    {
        /// <summary>
        /// The settings collection
        /// </summary>
        private NameValueCollection _settings;

        private HashSet<string> _encryptedSettings;

        /// <summary>
        /// The collection of matching descruptions
        /// </summary>
        private NameValueCollection _descriptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="NameValueSettings"/> class.
        /// </summary>
        public NameValueSettings()
            : this(new NameValueCollection(StringComparer.Ordinal))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NameValueSettings"/> class.
        /// </summary>
        /// <param name="source">The source to use.</param>
        internal NameValueSettings(NameValueCollection source)
        {
            _settings = source;
            _descriptions = new NameValueCollection(StringComparer.Ordinal);
            _encryptedSettings = new HashSet<string>(StringComparer.Ordinal);
        }

        /// <summary>
        /// Gets the number of settings stored
        /// </summary>
        public int Count
        {
            get { return _settings.Count; }
        }

        /// <summary>
        /// Gets names of the stored settings
        /// </summary>
        public ICollection SettingNames
        {
            get { return _settings.Keys; }
        }

        internal NameValueCollection RawSettings
        {
            get { return _settings; }
        }

        /// <overloads>Gets a configuration setting.</overloads>
        /// <summary>
        /// Gets the specified string config setting.
        /// </summary>
        /// <param name="setting">The setting to retrieve</param>
        /// <param name="defaultValue">The default value to return if the setting is not found.</param>
        /// <returns>The requested setting's value.</returns>
        /// <exception cref="ArgumentNullException">If setting is null.</exception>
        public string Get(Enum setting, string defaultValue)
        {
            Ensure.ArgumentNotNull(setting, "setting");

            return Get(setting.ToString(), defaultValue);
        }

        /// <summary>
        /// Gets the specified string config setting.
        /// </summary>
        /// <param name="setting">The setting to retrieve</param>
        /// <param name="defaultValue">The default value to return if the setting is not found.</param>
        /// <returns>The requested setting's value.</returns>
        /// <exception cref="ArgumentNullException">If setting is null or empty.</exception>
        public string Get(string setting, string defaultValue)
        {
            Ensure.ArgumentNotNullOrEmpty(setting, "setting");

            string val = _settings[setting];
            if (string.IsNullOrEmpty(val))
            {
                return defaultValue;
            }

            return val;
        }

        /// <summary>
        /// Gets the specified setting.
        /// </summary>
        /// <typeparam name="T">The type of the setting</typeparam>
        /// <param name="setting">The setting to retrieve.</param>
        /// <param name="defaultValue">The default value to return if the setting is not found.</param>
        /// <returns>The requested setting's value.</returns>
        /// <exception cref="ArgumentNullException">If setting is null.</exception>
        /// <exception cref="ConfigException">If the setting cannot be read as the requested type.</exception>
        public T Get<T>(Enum setting, T defaultValue)
        {
            Ensure.ArgumentNotNull(setting, "setting");

            return Get<T>(setting.ToString(), defaultValue);
        }

        /// <summary>
        /// Gets the specified setting.
        /// </summary>
        /// <typeparam name="T">The type of the setting</typeparam>
        /// <param name="setting">The setting to retrieve.</param>
        /// <param name="defaultValue">The default value to return if the setting is not found.</param>
        /// <returns>The requested setting's value.</returns>
        /// <exception cref="ArgumentNullException">If setting is null or empty.</exception>
        /// <exception cref="ConfigException">If the setting cannot be read as the requested type.</exception>
        public T Get<T>(string setting, T defaultValue)
        {
            Ensure.ArgumentNotNullOrEmpty(setting, "setting");

            return (T)GetSetting<T>(setting, defaultValue);
        }

        /// <overloads>Sets an encrypted configuration setting.</overloads>
        /// <summary>
        /// Sets the specified encrypted config setting.
        /// </summary>
        /// <param name="setting">The setting to set.</param>
        /// <param name="value">The new value.</param>
        /// <exception cref="ArgumentNullException">If setting is null.</exception>
        public void SetEncryptedSetting(Enum setting, object value)
        {
            SetSetting(setting, value, true);
        }

        /// <summary>
        /// Sets the specified encrypted config setting.
        /// </summary>
        /// <param name="setting">The setting to set.</param>
        /// <param name="value">The new value.</param>
        /// <exception cref="ArgumentNullException">If setting is null.</exception>
        public void SetEncryptedSetting(string setting, object value)
        {
            SetSetting(setting, value, true);
        }

        /// <overloads>Sets a configuration setting.</overloads>
        /// <summary>
        /// Sets the specified config setting.
        /// </summary>
        /// <param name="setting">The setting to set.</param>
        /// <param name="value">The new value.</param>
        /// <exception cref="ArgumentNullException">If setting is null.</exception>
        public void SetSetting(Enum setting, object value)
        {
            SetSetting(setting, value, false);
        }

        /// <summary>
        /// Sets the specified config setting.
        /// </summary>
        /// <param name="setting">The setting to set.</param>
        /// <param name="value">The new value.</param>
        /// <param name="encrypt">true to encrypt the value, otherwise false.</param>
        /// <exception cref="ArgumentNullException">If setting is null.</exception>
        public void SetSetting(Enum setting, object value, bool encrypt)
        {
            Ensure.ArgumentNotNull(setting, "setting");

            SetSetting(setting.ToString(), value, encrypt);
        }

        /// <summary>
        /// Sets the specified config setting.
        /// </summary>
        /// <param name="setting">The setting to set.</param>
        /// <param name="value">The new value.</param>
        /// <exception cref="ArgumentException">If setting is null or empty.</exception>
        public void SetSetting(string setting, object value)
        {
            SetSetting(setting, value, false);
        }

        /// <summary>
        /// Sets the specified config setting.
        /// </summary>
        /// <param name="setting">The setting to set.</param>
        /// <param name="value">The new value.</param>
        /// <param name="encrypt">true to encrypt the value, otherwise false.</param>
        /// <exception cref="ArgumentException">If setting is null or empty.</exception>
        public void SetSetting(string setting, object value, bool encrypt)
        {
            Ensure.ArgumentNotNullOrEmpty(setting, "setting");

            string newValue = (value != null) ? StringifyValue(value) : string.Empty;

            lock (_settings)
            {
                if (encrypt)
                {
                    _encryptedSettings.Add(setting);
                }
                else
                {
                    _encryptedSettings.Remove(setting);
                }

                _settings[setting] = newValue;
            }
        }

        /// <overloads>Get the description associated with the setting.</overloads>
        /// <summary>
        /// Get the description associated with the setting.
        /// </summary>
        /// <param name="setting">The setting to get the description of</param>
        /// <returns>The description of the setting</returns>
        public string GetDescription(Enum setting)
        {
            Ensure.ArgumentNotNull(setting, "setting");

            return GetDescription(setting.ToString());
        }

        /// <summary>
        /// Get the description associated with the setting.
        /// </summary>
        /// <param name="setting">The setting to get the description of</param>
        /// <returns>The description of the setting</returns>
        public string GetDescription(string setting)
        {
            Ensure.ArgumentNotNullOrEmpty(setting, "setting");

            lock (_descriptions)
            {
                return _descriptions[setting] ?? string.Empty;
            }
        }

        /// <overloads>Associates a description with a setting.</overloads>
        /// <summary>
        /// Associates a description with a setting.
        /// </summary>
        /// <param name="setting">The setting to describe</param>
        /// <param name="description">The description</param>
        public void SetDescription(Enum setting, string description)
        {
            Ensure.ArgumentNotNull(setting, "setting");

            SetDescription(setting.ToString(), description);
        }

        /// <summary>
        /// Associates a description with a setting.
        /// </summary>
        /// <param name="setting">The setting to describe</param>
        /// <param name="description">The description</param>
        public void SetDescription(string setting, string description)
        {
            Ensure.ArgumentNotNullOrEmpty(setting, "setting");

            lock (_descriptions)
            {
                _descriptions[setting] = description;
            }
        }

        /// <overloads>Removes a configuration setting.</overloads>
        /// <summary>
        /// Removes the specified config setting.
        /// </summary>
        /// <param name="setting">The setting to remove.</param>
        public void RemoveSetting(Enum setting)
        {
            if (setting == null)
            {
                return;
            }

            RemoveSetting(setting.ToString());
        }

        /// <summary>
        /// Removes the specified config setting.
        /// </summary>
        /// <param name="setting">The setting to remove.</param>
        public void RemoveSetting(string setting)
        {
            if (setting == null)
            {
                return;
            }

            lock (_settings)
            {
                _settings.Remove(setting);
                _encryptedSettings.Remove(setting);
            }
        }

        /// <overloads>Removes a setting description.</overloads>
        /// <summary>
        /// Removes the specified setting description.
        /// </summary>
        /// <param name="setting">The setting to remove the description from.</param>
        public void RemoveDescription(Enum setting)
        {
            if (setting == null)
            {
                return;
            }

            RemoveDescription(setting.ToString());
        }

        /// <summary>
        /// Removes the specified setting description.
        /// </summary>
        /// <param name="setting">The setting to remove the description from.</param>
        public void RemoveDescription(string setting)
        {
            if (setting == null)
            {
                return;
            }

            lock (_descriptions)
            {
                _descriptions.Remove(setting);
            }
        }

        /// <overloads>Determines if a given setting exists.</overloads>
        /// <summary>
        /// Determines if a given setting exists, meaning that it is in the config file and is not null or empty.
        /// </summary>
        /// <param name="setting">The setting to evaluate.</param>
        /// <returns>true if the setting was found, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">If setting is null.</exception>
        public bool HasSetting(Enum setting)
        {
            Ensure.ArgumentNotNull(setting, "setting");

            return HasSetting(setting.ToString());
        }

        /// <summary>
        /// Determines if a given setting exists, meaning that it is in the config file and is not null or empty.
        /// </summary>
        /// <param name="setting">The setting to evaluate.</param>
        /// <returns>true if the setting was found, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">If setting is null or empty.</exception>
        public bool HasSetting(string setting)
        {
            Ensure.ArgumentNotNullOrEmpty(setting, "setting");

            return !string.IsNullOrEmpty(_settings[setting]);
        }

        /// <overloads>Determines if a given setting is encrypted.</overloads>
        /// <summary>
        /// Determines if a given setting is encrypted.
        /// </summary>
        /// <param name="setting">The setting to evaluate.</param>
        /// <returns>true if the setting is encrypted, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">If setting is null.</exception>
        public bool IsEncrypted(Enum setting)
        {
            Ensure.ArgumentNotNull(setting, "setting");

            return IsEncrypted(setting.ToString());
        }

        /// <summary>
        /// Determines if a given setting is encrypted.
        /// </summary>
        /// <param name="setting">The setting to evaluate.</param>
        /// <returns>true if the setting is encrypted, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">If setting is null or empty.</exception>
        public bool IsEncrypted(string setting)
        {
            Ensure.ArgumentNotNullOrEmpty(setting, "setting");

            lock (_settings)
            {
                return _encryptedSettings.Contains(setting);
            }
        }

        private static string StringifyValue(object value)
        {
            Type settingType = value.GetType();

            if (settingType.Equals(typeof(DateTime)))
            {
                return XmlConvert.ToString((DateTime)value, XmlDateTimeSerializationMode.RoundtripKind);
            }
            else if (typeof(IConvertible).IsAssignableFrom(settingType))
            {
                return ((IConvertible)value).ToString(CultureInfo.InvariantCulture);
            }

            return value.ToString();
        }

        private object GetSetting<T>(string settingName, T defaultValue)
        {
            try
            {
                //Get the string value
                string stringValue = _settings[settingName];
                if (string.IsNullOrEmpty(stringValue))
                {
                    return defaultValue;
                }

                var settingType = typeof(T);

                //Get the actual typed value based on property type
                if (settingType.Equals(typeof(string)))
                {
                    return stringValue;
                }
                else if (settingType.IsEnum)
                {
                    return Enum.Parse(settingType, stringValue, true);
                }
                else if (settingType.Equals(typeof(DateTime)))
                {
                    return XmlConvert.ToDateTime(stringValue, XmlDateTimeSerializationMode.RoundtripKind);
                }
                else if (typeof(IConvertible).IsAssignableFrom(settingType))
                {
                    return Convert.ChangeType(stringValue, settingType, CultureInfo.InvariantCulture);
                }
                else if (settingType.Equals(typeof(Guid)))
                {
                    return XmlConvert.ToGuid(stringValue);
                }
                else if (settingType.Equals(typeof(TimeSpan)))
                {
                    return TimeSpan.Parse(stringValue, CultureInfo.InvariantCulture);
                }

                throw new ConfigException(
                    Msg.Text("Setting '{0}' requested as unsupported type '{1}'.", settingName, settingType.FullName),
                    ConfigErrorCode.InvalidConfigType);
            }
            catch (Exception e)
            {
                throw new ConfigException(
                    Msg.Text("Setting '{0}' has an invalid value. See nested exception for details.", settingName),
                    ConfigErrorCode.InvalidConfigValue,
                    e);
            }
        }
    }
}
