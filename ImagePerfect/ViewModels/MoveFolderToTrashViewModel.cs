using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ImagePerfect.Helpers;
using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace ImagePerfect.ViewModels
{
	public class MoveFolderToTrashViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly FolderMethods _folderMethods;
        private readonly MainWindowViewModel _mainWindowViewModel;
        public MoveFolderToTrashViewModel(IUnitOfWork unitOfWork, MainWindowViewModel mainWindowViewModel) 
		{
            _unitOfWork = unitOfWork;
            _mainWindowViewModel = mainWindowViewModel;
            _folderMethods = new FolderMethods(_unitOfWork);
        }

        public async Task MoveFolderToTrash(FolderViewModel folderVm)
        {
            //only allow delete if folder does not contain children/sub directories
            List<Folder> folderAndSubFolders = await _folderMethods.GetDirectoryTree(folderVm.FolderPath);
            if (folderAndSubFolders.Count > 1) 
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Delete Folder", "This folder contains sub folders clean those up first.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
            var boxYesNo = MessageBoxManager.GetMessageBoxStandard("Delete Folder", "Are you sure you want to delete your folder?", ButtonEnum.YesNo);
            var boxResult = await boxYesNo.ShowAsync();
            if (boxResult == ButtonResult.Yes) 
            {
                _mainWindowViewModel.ShowLoading = true;
                //the folders parent
                string pathThatContainsFolder = PathHelper.RemoveOneFolderFromPath(folderVm.FolderPath);                
                Folder? rootFolder = await _folderMethods.GetRootFolder();
                string trashFolderPath = PathHelper.GetTrashFolderPath(rootFolder.FolderPath);

                //create ImagePerfectTRASH if it doesnt exist
                if (!Directory.Exists(trashFolderPath))
                {
                    Directory.CreateDirectory(trashFolderPath);
                }
                if (Directory.Exists(folderVm.FolderPath))
                {
                    //delete folder from db -- does not delete sub folders.
                    //images table child of folders ON DELETE CASCADE is applied on sql to delete all images if a folder is deleted
                    bool success = await _folderMethods.DeleteFolder(folderVm.FolderId);
                    if (success) 
                    {
                        //move folder to trash folder
                        string newFolderPath = PathHelper.GetFolderTrashPath(folderVm, trashFolderPath);
                        Directory.Move(folderVm.FolderPath, newFolderPath);
                        //update the parent folder HasChildren prop
                        List<Folder> parentFolderDirTree = await _folderMethods.GetDirectoryTree(pathThatContainsFolder);
                        Folder parentFolder = await _folderMethods.GetFolderAtDirectory(pathThatContainsFolder);
                        if (parentFolderDirTree.Count > 1)
                        {
                            parentFolder.HasChildren = true;
                            await _folderMethods.UpdateFolder(parentFolder);
                        }
                        else
                        {
                            parentFolder.HasChildren = false;
                            await _folderMethods.UpdateFolder(parentFolder);
                        }
                        //refresh UI
                        await _mainWindowViewModel.RefreshFolders(pathThatContainsFolder);
                        _mainWindowViewModel.ShowLoading = false;
                    }
                }
            }
        }
	}
}