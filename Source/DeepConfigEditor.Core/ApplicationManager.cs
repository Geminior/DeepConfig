namespace DeepConfigEditor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Caliburn.Micro;
    using DeepConfig.Providers;
    using DeepConfigEditor.Actions;
    using DeepConfigEditor.Config;
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Messages;
    using DeepConfigEditor.Services;
    using Ninject;

    public sealed class ApplicationManager
    {
        private IActionsManager _actionsManager;

        public ApplicationManager(IActionsManager actionsManager)
        {
            _actionsManager = actionsManager;
        }

        public System.Action Initialize(IKernel container, string[] commandArgs)
        {
            object startView;

            if (commandArgs.Length > 0)
            {
                //First check if there are privileged actions to execute. If so we do that and no more and ask the application to close.
                if (_actionsManager.ExecuteCommandLine(commandArgs))
                {
                    return null;
                }

                var source = new ConfigSource(new FileConfigProvider(commandArgs[0]));

                startView = new ConfigRequest(
                    ConfigRequest.Action.Open,
                    source);
            }
            else
            {
                startView = new ConfigRequest(ConfigRequest.Action.StartScreen);
            }

            //Load plugins and update the assembly source cache accordingly
            var actionSources = LoadPlugins(container);

            //Build the action collections
            _actionsManager.BuildActions(actionSources);

            return () =>
                {
                    var msg = container.Get<IEventAggregator>();
                    msg.Publish(startView);
                };
        }

        private static IEnumerable<IProvideActions> LoadPlugins(IKernel container)
        {
            if (Execute.InDesignMode)
            {
                return new IProvideActions[0];
            }

            var pluginFolder = PluginConfig.Instance.PluginFolder;
            if (!Directory.Exists(pluginFolder))
            {
                return new IProvideActions[0];
            }

            var actionSources = new List<IProvideActions>();

            var pluginMarkerType = typeof(IPlugin);
            var actionProviderType = typeof(IProvideActions);
            var configProviderType = typeof(IProvideConfigSource);

            //Iterate through potentiel plugin assemblies and extract the plugins
            var possiblePlugins = Directory.EnumerateFiles(pluginFolder, "*.dll", SearchOption.AllDirectories);
            foreach (var filePath in possiblePlugins)
            {
                try
                {
                    bool hasPlugin = false;

                    var asm = LoadAssemblyFromPath(filePath.ToUpperInvariant());
                    foreach (var pluginType in asm.GetExportedTypes().Where(t => pluginMarkerType.IsAssignableFrom(t) && !t.IsAbstract))
                    {
                        if (configProviderType.IsAssignableFrom(pluginType))
                        {
                            container.Bind(configProviderType).To(pluginType).InTransientScope();
                        }
                        else if (actionProviderType.IsAssignableFrom(pluginType))
                        {
                            var p = Activator.CreateInstance(pluginType) as IProvideActions;
                            actionSources.Add(p);
                        }

                        //Even if the assembly has none of the above, it still has types marked as plugins so it should be added as a source
                        hasPlugin = true;
                    }

                    if (hasPlugin)
                    {
                        AssemblySource.Instance.Add(asm);
                    }
                }
                catch (Exception e)
                {
                    Logger.Instance.Error(
                        string.Format("Failed to load plugin assembly: {0}.", filePath),
                        e);
                }
            }

            return actionSources;
        }

        private static Assembly LoadAssemblyFromPath(string path)
        {
            AssemblyName assemblyName;

            try
            {
                assemblyName = AssemblyName.GetAssemblyName(path);
            }
            catch (ArgumentException)
            {
                assemblyName = new AssemblyName()
                {
                    CodeBase = path
                };
            }

            return Assembly.Load(assemblyName);
        }
    }
}
