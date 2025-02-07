using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        }
    }
}