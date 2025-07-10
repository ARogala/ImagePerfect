using System;
using System.Collections.Generic;
using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;
using ReactiveUI;

namespace ImagePerfect.ViewModels
{
	public class ModifyFolderDataViewModel : ViewModelBase
	{
        private readonly IUnitOfWork _unitOfWork;
        private readonly FolderMethods _folderMethods;
        private readonly MainWindowViewModel _mainWindowViewModel;
        public ModifyFolderDataViewModel(IUnitOfWork unitOfWork, MainWindowViewModel mainWindowViewModel) 
		{
            _unitOfWork = unitOfWork;
            _mainWindowViewModel = mainWindowViewModel;
            _folderMethods = new FolderMethods(_unitOfWork);
        }
	}
}