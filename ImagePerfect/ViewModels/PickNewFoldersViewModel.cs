using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using ReactiveUI;
using ImagePerfect.Helpers;
using System.Linq;
using System.Diagnostics;

namespace ImagePerfect.ViewModels
{
	public class PickNewFoldersViewModel : ViewModelBase
	{
        private readonly IUnitOfWork _unitOfWork;
        private readonly FolderCsvMethods _folderCsvMethods;
        private readonly FolderMethods _folderMethods;
        private readonly MainWindowViewModel _mainWindowViewModel;
        public PickNewFoldersViewModel(IUnitOfWork unitOfWork, MainWindowViewModel mainWindowViewModel) 
		{
            _unitOfWork = unitOfWork;
            _folderMethods = new FolderMethods(_unitOfWork);
            _folderCsvMethods = new FolderCsvMethods(_unitOfWork);
            _mainWindowViewModel = mainWindowViewModel;
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

            _NewFolders = await _SelectNewFoldersInteraction.Handle(rootFolder.FolderPath);
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
            //check for parent folder -- must add parent folder 1st to prevent double import
            string parentDirectory = PathHelper.RemoveOneFolderFromPath(PathHelper.FormatPathFromFolderPicker(_NewFolders[0]));
            List<Folder> parentFolderDirTree = await _folderMethods.GetDirectoryTree(parentDirectory);
            if (parentFolderDirTree.Count == 0)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Add Folders", "Add the parent folder first.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
            _mainWindowViewModel.ShowLoading = true;
            //check if any folders are already in db
            List<Folder> allFoldersInDb = await _folderMethods.GetAllFolders();
            List<string> foldersNotToAdd = new List<string>();
            for (int i = 0; i < _NewFolders.Count; i++) 
            {
                foreach (Folder folder in allFoldersInDb)
                {
                    if (folder.FolderPath == PathHelper.FormatPathFromFolderPicker(_NewFolders[i]))
                    {
                        foldersNotToAdd.Add(_NewFolders[i]);
                    }
                }
            }
            foreach (string path in foldersNotToAdd)
            {
                _NewFolders.Remove(path);
            }
            if (_NewFolders.Count == 0) 
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Add Folders", "All the folders selected are already in the library.", ButtonEnum.Ok);
                await box.ShowAsync();
                _mainWindowViewModel.ShowLoading = false;
                return;
            }
            //build csv
            bool csvIsSet = await FolderCsvMethods.AddNewFoldersCsv(_NewFolders, false);
            //write csv to database
            if (csvIsSet) 
            {
                await _folderCsvMethods.AddFolderCsv();
                //reload the page
                await _mainWindowViewModel.RefreshFolders();
            }
            _mainWindowViewModel.ShowLoading = false;
        }
    }
}