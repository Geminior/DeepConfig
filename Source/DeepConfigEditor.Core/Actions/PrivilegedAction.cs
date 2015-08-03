namespace DeepConfigEditor.Actions
{
    using DeepConfigEditor.Extensions;

    internal abstract class PrivilegedAction : IAction
    {
        internal const string CommandPrefix = "-";
        internal const char ParamsPrefix = ':';

        private bool _canExecute;

        internal PrivilegedAction(IActionContext context, string cmdArg)
        {
            this.CommandArgument = cmdArg;
            _canExecute = context.IsAdministrator;
        }

        public bool CanExecute
        {
            get { return _canExecute; }
        }

        string IAction.InputGesture
        {
            get;
            set;
        }

        internal string CommandArgument
        {
            get;
            private set;
        }

        public abstract void Execute();

        void IAction.HandleCurrentConfigurationChanged()
        {
        }

        internal virtual bool Initialize(string cmdParams)
        {
            return true;
        }

        internal string CreateCommandArg()
        {
            var parameters = CreateCommandParameters();

            if (string.IsNullOrEmpty(parameters))
            {
                return string.Concat(CommandPrefix, this.CommandArgument);
            }

            return string.Concat(CommandPrefix, this.CommandArgument, ParamsPrefix, parameters);
        }

        protected virtual string CreateCommandParameters()
        {
            return null;
        }
    }
}
