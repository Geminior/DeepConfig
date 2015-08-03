namespace DeepConfigEditor.Extensions
{
    using System;
    using System.Windows;
    using DeepConfig.Providers;

    /// <summary>
    /// Interface that viewmodels for custom config provider implementations must implement.
    /// </summary>
    public interface IProvideConfigSource : IPlugin
    {
        /// <summary>
        /// Gets the icon URI of this source provider
        /// </summary>
        /// <value>
        /// The icon URI.
        /// </value>
        Uri IconUri { get; }

        /// <summary>
        /// Gets the display name of this source provider
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        string DisplayName { get; }

        /// <summary>
        /// Gets a value indicating whether source is ready, i.e. if <see cref="GetSource"/> can be called.
        /// </summary>
        /// <value>
        ///   <c>true</c> if <see cref="GetSource"/> can be called; otherwise, <c>false</c>.
        /// </value>
        bool CanGetSource { get; }
        
        /// <summary>
        /// Initializes the viewmodel with the purpose it is requested to serve. If it cannot satisfy the request, return <c>false</c> which will prevent the provider from showing as an option.
        /// </summary>
        /// <param name="p">The purpose.</param>
        /// <param name="provideSource">A callback that can be used to directly provide the source without awaiting a call to <see cref="GetSource" /></param>
        /// <returns><c>true</c> if the request can be satisfied, otherwise <c>false</c></returns>
        bool Initialize(ConfigSourcePurpose p, Action<ConfigSource> provideSource);

        /// <summary>
        /// Gets the provider.
        /// </summary>
        /// <returns>The <see cref="DeepConfig.Providers.IConfigProvider"/> to open a configuration from or save a configuration to.</returns>
        ConfigSource GetSource();
    }
}
