namespace DeepConfigEditor.Services
{
    using Caliburn.Micro.Extras;

    public interface IFileService
    {
        IOpenFileService OpenService { get; }

        ISaveFileService SaveService { get; }
    }
}
