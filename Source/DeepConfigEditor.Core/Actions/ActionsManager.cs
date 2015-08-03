namespace DeepConfigEditor.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Caliburn.Micro;
    using DeepConfigEditor.Config;
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Messages;
    using DeepConfigEditor.Resources;
    using DeepConfigEditor.Services;

    public class ActionsManager : IActionsManager, IHandle<ActiveConfigurationChangedMessage>, IHandle<MruListChangedMessage>
    {
        private readonly IActionContext _context;
        private readonly ActionItemParent _mruParent;
        private BindableCollection<IActionItem> _menuItems;
        private BindableCollection<IActionItem> _toolbarItems;
        private IList<IAction> _allExecutables;

        public ActionsManager(IActionContext context)
        {
            _context = context;

            _context.Messenger.Subscribe(this);
            _mruParent = new ActionItemParent(_context, ActionsRes.MruCaption);
        }

        public IEnumerable<IActionItem> MenuItems
        {
            get { return _menuItems; }
        }

        public IEnumerable<IActionItem> ToolBarItems
        {
            get { return _toolbarItems; }
        }

        public IActionContext ActionContext
        {
            get { return _context; }
        }

        public IEnumerable<IAction> AllExecutables
        {
            get
            {
                if (_allExecutables == null)
                {
                    _allExecutables = GetAllExecutables().ToList();
                }

                return _allExecutables;
            }
        }

        public void BuildActions(IEnumerable<IProvideActions> pluginActions)
        {
            //Define all the shared application actions
            var sep = new ActionItemSeparator();

            var newAction = new NewAction(_context);
            var openAction = new OpenAction(_context);
            var saveAction = new SaveAction(_context);
            var refreshAction = new RefreshAction(_context);

            //Define menus
            var sourceMenu = new ActionItemParent(_context, ActionsRes.SourceCaption)
                .AddRange(
                    newAction,
                    openAction,
                    saveAction,
                    new SaveAsAction(_context),
                    sep,
                    new AssignCryptoAction(_context),
                    sep,
                    _mruParent,
                    sep,
                    new ExitAction(_context));

            var viewMenu = new ActionItemParent(_context, ActionsRes.ViewCaption)
                .AddRange(
                    new ExpandAction(_context),
                    new CollapseAction(_context),
                    sep,
                    refreshAction);

            var optionsMenu = new ActionItemParent(_context, ActionsRes.OptionsHeaderCaption)
                .AddRange(
                    new AssociateAction(_context),
                    new ShowUnsupportedAction(_context),
                    new AlwaysOnTopAction(_context),
                    sep,
                    new OptionsAction(_context));

            var pluginMenu = new ActionItemParent(_context, ActionsRes.ExtensionsCaption);

            var helpMenu = new ActionItemParent(_context, ActionsRes.HelpCaption)
                .AddRange(
                    new AboutAction(_context));

            //Instantiate the collections
            _menuItems = new BindableCollection<IActionItem>()
                .AddRange(
                    sourceMenu,
                    viewMenu,
                    optionsMenu,
                    pluginMenu,
                    helpMenu);

            _toolbarItems = new BindableCollection<IActionItem>()
                .AddRange(
                    newAction,
                    openAction,
                    saveAction,
                    new DeleteAction(_context),
                    sep,
                    refreshAction,
                    sep,
                    new AddSectionAction(_context));

            //Create the plugin actions
            foreach (var p in pluginActions)
            {
                _toolbarItems.Add(sep);
                pluginMenu.AddRange(p.GetMenuActions(_context));
                _toolbarItems.AddRange(p.GetToolBarActions(_context));
            }

            pluginMenu.IsVisible = pluginMenu.ChildActions.Any();

            //Init mru
            UpdateMruList();
        }

        public bool ExecuteCommandLine(string[] args)
        {
            var cmds = (from cmd in args
                        where cmd.StartsWith(PrivilegedAction.CommandPrefix)
                        let tokens = cmd.Split(PrivilegedAction.ParamsPrefix)
                        select new
                        {
                            Key = tokens[0].Substring(1),
                            Params = tokens.ElementAtOrDefault(1)
                        }).ToList();

            if (!cmds.Any())
            {
                return false;
            }

            var ptype = typeof(PrivilegedAction);
            var availableActions = AssemblySource.Instance
                                    .SelectMany(a => a.GetTypes())
                                    .Where(t => ptype.IsAssignableFrom(t) && !t.IsAbstract)
                                    .Select(t => Activator.CreateInstance(t, true) as PrivilegedAction)
                                    .ToDictionary(p => p.CommandArgument);

            PrivilegedAction action;
            foreach (var cmd in cmds)
            {
                if (!availableActions.TryGetValue(cmd.Key, out action))
                {
                    Logger.Instance.Warn("No action matches the command argument '{0}'.", cmd.Key);
                    continue;
                }

                if (!action.CanExecute)
                {
                    Logger.Instance.Warn("The command '{0}' requires admin rights to run.", cmd.Key);
                    continue;
                }

                try
                {
                    if (action.Initialize(cmd.Params))
                    {
                        action.Execute();
                    }
                }
                catch (Exception e)
                {
                    Logger.Instance.Error(
                        string.Format("Failed to execute command '{0}' with args '{1}'", cmd.Key, cmd.Params),
                        e);
                }
            }

            return true;
        }

        public void Handle(ActiveConfigurationChangedMessage message)
        {
            var actions = this.AllExecutables;
            if (actions == null)
            {
                return;
            }

            actions.Apply(a => a.HandleCurrentConfigurationChanged());

            UpdateMruList();
        }

        public void Handle(MruListChangedMessage message)
        {
            UpdateMruList();
        }

        private void UpdateMruList()
        {
            var cfg = EditorSettings.Instance;
            _mruParent.ChildActions.Clear();

            foreach (var source in cfg.RecentSources.Take(cfg.RecentInMenu))
            {
                _mruParent.ChildActions.Add(new MruAction(_context, source));
            }
        }

        private IEnumerable<IAction> GetAllExecutables()
        {
            var menuExecutables = GetAllExecutablesRecursive(_menuItems);
            var toolbarExecutables = _toolbarItems.OfType<IAction>();

            return menuExecutables.Union(toolbarExecutables);
        }

        private IEnumerable<IAction> GetAllExecutablesRecursive(IEnumerable<IActionItem> source)
        {
            if (source == null)
            {
                return null;
            }

            //TODO: exclude mruParent from this, there is no need to update those items
            //Note there is no duplicate check here since only menu items are added and it is assumed that no duplicates exist there. Should that assumption prove wrong well...
            var subItems = source.OfType<ActionItemParent>().SelectMany(p => p.ChildActions);

            if (subItems.Any())
            {
                return source.OfType<IAction>().Concat(GetAllExecutablesRecursive(subItems));
            }

            return source.OfType<IAction>();
        }
    }
}
