using System;
using System.Collections.Generic;
using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using ReactiveUI;
using ImagePerfect.ObjectMappers;
using System.Linq;

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
    }
}