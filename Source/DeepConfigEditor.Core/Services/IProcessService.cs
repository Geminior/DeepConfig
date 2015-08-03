namespace DeepConfigEditor.Services
{
    public interface IProcessService
    {
        bool IsRunningAsAdministrator { get; }

        bool RunAsAdministrator(string command);
    }
}
