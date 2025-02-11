using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using ReactiveUI;
using ImagePerfect.Helpers;
using System.Collections.ObjectModel;
using ImagePerfect.ObjectMappers;

namespace ImagePerfect.ViewModels
{
	public class PickNewFoldersViewModel : ViewModelBase
	{
        private readonly IUnitOfWork _unitOfWork;
        private readonly FolderCsvMethods _folderCsvMethods;
        private readonly FolderMethods _folderMethods;
        private ObservableCollection<FolderViewModel> _libraryFolders;
        public PickNewFoldersViewModel(IUnitOfWork unitOfWork, ObservableCollection<FolderViewModel> LibraryFolders) 
		{
            _unitOfWork = unitOfWork;
            _folderMethods = new FolderMethods(_unitOfWork);
            _folderCsvMethods = new FolderCsvMethods(_unitOfWork);
            _libraryFolders = LibraryFolders;
            _SelectNewFoldersInteraction = new Interaction<string, List<string>?>();
			SelectNewFoldersCommand = ReactiveCommand.CreateFromTask(SelectNewFolders);
		}

		private List<string>? _NewFolders;

		private readonly Interaction<string, List<string>?> _SelectNewFoldersInteraction;

        public Interaction<string, List<string>?> SelectNewFoldersInteraction { get { return _SelectNewFoldersInteraction; } }

		public ReactiveCommand<Unit, Unit> SelectNewFoldersCommand { get; }

		private async Task SelectNewFolders()
		{
            Folder? rootFolder = await _folderMethods.GetRootFolder();
            if (rootFolder == null)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Add Folders", "You need to add a root library folder first before new folders can be added to it.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
            
            _NewFolders = await _SelectNewFoldersInteraction.Handle("Select New Folders");
			//list will be empty if Cancel is pressed exit method
			if (_NewFolders.Count == 0) 
			{
				return;
			}
            //add check to make sure user is picking folders within the root libary directory
            string pathCheck = PathHelper.FormatPathFromFolderPicker(_NewFolders[0]);
            if (!pathCheck.Contains(rootFolder.FolderPath))
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Add Folders", "You can only add folders that are within your root library folder.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
            //build csv
            bool csvIsSet = await FolderCsvMethods.AddNewFoldersCsv(_NewFolders);
            //write csv to database
            if (csvIsSet) 
            {
                await _folderCsvMethods.AddFolderCsv();
                //reload the page
                _libraryFolders.Clear();
                FolderViewModel rootFolderVm = await FolderMapper.GetFolderVm(rootFolder);
                _libraryFolders.Add(rootFolderVm);
            }
        }
    }
}