using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ImagePerfect.Helpers;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using ReactiveUI;
using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;
using ImagePerfect.Repository;
using System.Diagnostics;

namespace ImagePerfect.ViewModels
{
	public class PickMoveToFolderViewModel : ViewModelBase
	{
        private readonly IUnitOfWork _unitOfWork;
        private readonly FolderMethods _folderMethods;
        private readonly ImageMethods _imageMethods;
        public PickMoveToFolderViewModel(IUnitOfWork unitOfWork) 
		{
            _unitOfWork = unitOfWork;
            _folderMethods = new FolderMethods(_unitOfWork);
            _imageMethods = new ImageMethods(_unitOfWork);

            _SelectMoveToFolderInteration = new Interaction<string, List<string>?>();
			SelectMoveToFolderCommand = ReactiveCommand.CreateFromTask((FolderViewModel folderVm) => SelectMoveToFolder(folderVm));
		}

		private List<string>? _MoveToFolderPath;

		private Interaction<string, List<string>?> _SelectMoveToFolderInteration;

		public Interaction<string, List<string>?> SelectMoveToFolderInteration { get { return _SelectMoveToFolderInteration; } }

		public ReactiveCommand<FolderViewModel, Unit> SelectMoveToFolderCommand { get; }

		private async Task SelectMoveToFolder(FolderViewModel folderVm)
		{
            Folder? rootFolder = await _folderMethods.GetRootFolder();
            if (rootFolder == null)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Move Folder", "You need to add a root library folder first before you can move a folder in it.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }

            _MoveToFolderPath = await _SelectMoveToFolderInteration.Handle("Select Folder To Move To");
			//list will be empty if Cancel is pressed exit method
			if (_MoveToFolderPath.Count == 0) 
			{ 
				return;
			}
            //add check to make sure user is picking folders within the root libary directory
            string pathCheck = PathHelper.FormatPathFromFolderPicker(_MoveToFolderPath[0]);
            if (!pathCheck.Contains(rootFolder.FolderPath))
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Move Folder", "You can only move folders that are within your root library folder.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
            //Cannot move folder to one of its subfolders
            if (pathCheck.Contains(folderVm.FolderPath))
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Move Folder", "The destination folder is a subfolder of the source folder. Cannot do this.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
            //move folder in db
            string newFolderPath = PathHelper.FormatPathFromFolderPicker(_MoveToFolderPath[0]);
           
            Debug.WriteLine("current folder path " + folderVm.FolderPath);
            Debug.WriteLine("picked folder path " + newFolderPath);
            Debug.WriteLine(PathHelper.GetRegExpStringForSubDirectories(folderVm.FolderPath));

            //pull current folder and sub folders from db
            List<Folder> folders = await _folderMethods.GetFoldersInDirectory(PathHelper.GetRegExpStringForSubDirectories(folderVm.FolderPath));
            List<Image> images = await _imageMethods.GetImagesInDirectory(PathHelper.GetRegExpStringForSubDirectories(folderVm.FolderPath));
           
            //modify folder path and folder, cover image path, and images
            folders = PathHelper.ModifyFolderPathsForFolderMove(folders, folderVm.FolderName, newFolderPath);
            images = PathHelper.ModifyImagePathsForFolderMove(images, folderVm.FolderName, newFolderPath);


            //build sql string and update db
            PathHelper.BuildFolderSqlForFolderMove(folders);
            //move images in db same basic idea as folders do both in a transaction -- maybe even move the pull colder and image from db to txn?

            //move folder in filesystem if db move is successfull
        }
    }
}