using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ImagePerfect.Repository.IRepository;
using ReactiveUI;

namespace ImagePerfect.ViewModels
{
    public class PickFolderCoverImageViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private ObservableCollection<FolderViewModel> _libraryFolders;
        public PickFolderCoverImageViewModel(IUnitOfWork unitOfWork, ObservableCollection<FolderViewModel> LibraryFolders)
        {
            _unitOfWork = unitOfWork;
            _libraryFolders = LibraryFolders;

            _SelectCoverImageInteraction = new Interaction<string, List<string>?>();
            SelectCoverImageCommand = ReactiveCommand.CreateFromTask(SelectCoverImage);
        }

        private List<String>? _CoverImagePath;

        private readonly Interaction<string, List<string>?> _SelectCoverImageInteraction;

        public Interaction<string, List<string>?> SelectCoverImageInteraction { get { return _SelectCoverImageInteraction; } }

        public ReactiveCommand<Unit, Unit> SelectCoverImageCommand { get; }

        private async Task SelectCoverImage()
        {
            _CoverImagePath = await _SelectCoverImageInteraction.Handle("Select Folder Cover Image");
            //list will be empty if Cancel is pressed exit method
            if (_CoverImagePath.Count == 0) 
            { 
                return;
            }
            Debug.WriteLine(_CoverImagePath[0]);
        }
    }
}