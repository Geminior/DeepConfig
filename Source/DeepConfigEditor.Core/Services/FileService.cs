namespace DeepConfigEditor.Services
{
    using System;
    using Caliburn.Micro.Extras;

    public class FileService : IFileService
    {
        private Lazy<IOpenFileService> _openService;
        private Lazy<ISaveFileService> _saveService;

        public FileService()
        {
            _openService = new Lazy<IOpenFileService>(() => new OpenFileService());
            _saveService = new Lazy<ISaveFileService>(() => new SaveFileService());
        }

        public IOpenFileService OpenService
        {
            get { return _openService.Value; }
        }

        public ISaveFileService SaveService
        {
            get { return _saveService.Value; }
        }
    }
}
