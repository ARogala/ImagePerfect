using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;
using System.Collections.ObjectModel;

namespace ImagePerfect.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public MainWindowViewModel() { }
        public MainWindowViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public PickRootFolderViewModel PickRootFolder { get => new PickRootFolderViewModel(_unitOfWork); }

        public ObservableCollection<Folder> LibraryFolders { get; } = new ObservableCollection<Folder>();

        private async void GetAllLibraryFolders()
        {

        }
    }
}
