namespace ImagePerfect.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public PickRootFolderViewModel PickRootFolder{ get; } = new PickRootFolderViewModel();
    }
}
