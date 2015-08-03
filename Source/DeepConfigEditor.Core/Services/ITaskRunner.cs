namespace DeepConfigEditor.Services
{
    using System;
    using System.Threading.Tasks;

    public interface ITaskRunner
    {
        Task<bool> ExecuteWithDialogOnError(Task t, string messageOnError);
    }
}
