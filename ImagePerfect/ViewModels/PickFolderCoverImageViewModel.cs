using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ImagePerfect.Helpers;
using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using ReactiveUI;

namespace ImagePerfect.ViewModels
{
    public class PickFolderCoverImageViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly FolderMethods _folderMethods;
        private readonly MainWindowViewModel _mainWindowViewModel;
        public PickFolderCoverImageViewModel(IUnitOfWork unitOfWork, MainWindowViewModel mainWindowViewModel)
        {
            _unitOfWork = unitOfWork;
            _mainWindowViewModel = mainWindowViewModel;
            _folderMethods = new FolderMethods(_unitOfWork);
            _SelectCoverImageInteraction = new Interaction<string, List<string>?>();
            SelectCoverImageCommand = ReactiveCommand.CreateFromTask((FolderViewModel folderVm) => SelectCoverImage(folderVm));
        }

        private List<String>? _CoverImagePath;

        private readonly Interaction<string, List<string>?> _SelectCoverImageInteraction;

        public Interaction<string, List<string>?> SelectCoverImageInteraction { get { return _SelectCoverImageInteraction; } }

        public ReactiveCommand<FolderViewModel, Unit> SelectCoverImageCommand { get; }

        private async Task SelectCoverImage(FolderViewModel folderVm)
        {
            _CoverImagePath = await _SelectCoverImageInteraction.Handle(folderVm.FolderPath);
            //list will be empty if Cancel is pressed exit method
            if (_CoverImagePath.Count == 0) 
            { 
                return;
            }
            //add check to make sure user is picking cover image directly in the folder
            //the move operation is complex and best to ensure each cover image is an image from its own folder only
            string pathCheck = PathHelper.FormatPathFromFolderPicker(_CoverImagePath[0]);
            //pathCheck is now the selected image folder path.
            pathCheck = PathHelper.RemoveOneFolderFromPath(pathCheck);
            if (pathCheck != folderVm.FolderPath)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Cover Image", "You can only select a cover image that is within its own folder.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
            bool success = await _folderMethods.UpdateCoverImage(PathHelper.FormatPathFromFilePicker(_CoverImagePath[0]), folderVm.FolderId);
            //update lib folders to show the new cover !!
            if (success)
            {
                string foldersDirectoryPath = PathHelper.RemoveOneFolderFromPath(folderVm.FolderPath);
                await _mainWindowViewModel.RefreshFolderProps(foldersDirectoryPath, folderVm);
            }
        }
    }
}