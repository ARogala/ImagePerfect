using System;
using System.Collections.Generic;
using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using ReactiveUI;
using ImagePerfect.ObjectMappers;
using System.Linq;
using System.Threading.Tasks;

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

        public async void UpdateFolder(FolderViewModel folderVm, string fieldUpdated)
        {
            Folder folder = FolderMapper.GetFolderFromVm(folderVm);
            bool success = await _folderMethods.UpdateFolder(folder);
            if (!success)
            {
                var box = MessageBoxManager.GetMessageBoxStandard($"Add {fieldUpdated}", $"Folder {fieldUpdated} update error. Try again.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
        }

        public async void EditFolderTag(FolderViewModel folderVm)
        {
            if (folderVm.FolderTags == null || folderVm.FolderTags == "")
            {
                if (folderVm.Tags.Count == 1)
                {
                    await _folderMethods.DeleteFolderTag(folderVm.Tags[0]);
                }
                else if (folderVm.Tags.Count == 0)
                {
                    return;
                }
            }
            List<string> folderTags = folderVm.FolderTags.Split(",").ToList();
            FolderTag? tagToRemove = null;
            foreach (FolderTag tag in folderVm.Tags)
            {
                if (!folderTags.Contains(tag.TagName))
                {
                    tagToRemove = tag;
                }
            }
            if (tagToRemove != null)
            {
                await _folderMethods.DeleteFolderTag(tagToRemove);
            }
        }

        public async void AddFolderTag(FolderViewModel folderVm)
        {
            //click submit with empty input just return
            if (folderVm.NewTag == "" || folderVm.NewTag == null)
            {
                return;
            }
            Folder folder = FolderMapper.GetFolderFromVm(folderVm);
            //update folder table and tags table in db -- success will be false if you try to input a duplicate tag
            bool success = await _folderMethods.UpdateFolderTags(folder, folderVm.NewTag);
            if (success)
            {
                //Update TagsList to show in UI AutoCompleteBox clear NewTag in box as well and refresh folders to show new tag
                await _mainWindowViewModel.GetTagsList();
                folderVm.NewTag = "";
                //refresh UI
                await _mainWindowViewModel.RefreshFolderProps(_mainWindowViewModel.CurrentDirectory, folderVm);
            }
            else
            {
                folderVm.NewTag = "";
            }
        }

        public async Task RemoveTagOnAllFolders(Tag selectedTag)
        {
            //nothing selected just return
            if (selectedTag == null)
                return;
            var boxYesNo = MessageBoxManager.GetMessageBoxStandard("Remove Tag", "CAUTION you are about to remove a tag this could take a long time are you sure?", ButtonEnum.YesNo);
            var boxResult = await boxYesNo.ShowAsync();
            if (boxResult != ButtonResult.Yes)
                return;

            _mainWindowViewModel.ShowLoading = true;
            try
            {
                //select all folders from db with tag as List<Folder>
                (List<Folder> folders, List<FolderTag> tags) folderTagResult = await _folderMethods.GetAllFoldersWithTag(selectedTag.TagName, false, _mainWindowViewModel.CurrentDirectory);
                List<Folder> taggedFolders = folderTagResult.folders;
                //no taggedFolders returned just exit
                if(taggedFolders == null || taggedFolders.Count == 0)
                    return;

                //remove tag from database
                await _folderMethods.RemoveTagOnAllFolders(selectedTag);
                //Update TagsList to show in UI
                await _mainWindowViewModel.GetTagsList();
            }
            finally
            {
                _mainWindowViewModel.ShowLoading = false;
            }
        }
    }
}