using ImagePerfect.Repository.IRepository;

namespace ImagePerfect.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private PickRootFolderViewModel _pickRootFolderViewModel;
        public MainWindowViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _pickRootFolderViewModel = new PickRootFolderViewModel(_unitOfWork);
        }
        public PickRootFolderViewModel PickRootFolder { get => _pickRootFolderViewModel; }
    }
}
