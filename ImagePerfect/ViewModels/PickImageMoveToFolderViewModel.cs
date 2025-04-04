using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using ReactiveUI;
using ImagePerfect.Helpers;
using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;
using System.Diagnostics;

namespace ImagePerfect.ViewModels
{
	public class PickImageMoveToFolderViewModel : ViewModelBase
	{
        private readonly IUnitOfWork _unitOfWork;
        private readonly FolderMethods _folderMethods;
        private readonly MainWindowViewModel _mainWindowViewModel;
        public PickImageMoveToFolderViewModel(IUnitOfWork unitOfWork, MainWindowViewModel mainWindowViewModel)
        {
            _unitOfWork = unitOfWork;
            _mainWindowViewModel = mainWindowViewModel;
            _folderMethods = new FolderMethods(_unitOfWork);

            _SelectMoveImagesToFolderInteration = new Interaction<string, List<string>?>();
            SelectMoveImagesToFolderCommand = ReactiveCommand.CreateFromTask(()=>SelectMoveImagesToFolder());
        }

        private List<string>? _MoveImagesToFolderPath;
        
        private Interaction<string, List<string>?> _SelectMoveImagesToFolderInteration;

        public Interaction<string, List<string>?> SelectMoveImagesToFolderInteration { get { return _SelectMoveImagesToFolderInteration; } }

        public ReactiveCommand<Unit, Unit> SelectMoveImagesToFolderCommand { get; }

        private async Task SelectMoveImagesToFolder()
        {
            Folder? rootFolder = await _folderMethods.GetRootFolder();
            _MoveImagesToFolderPath = await _SelectMoveImagesToFolderInteration.Handle(_mainWindowViewModel.CurrentDirectory);
            //list will be empty if Cancel is pressed exit method
            if (_MoveImagesToFolderPath.Count == 0) 
            { 
                return;
            }
            //add check to make sure user is picking folders within the root libary directory
            string pathCheck = PathHelper.FormatPathFromFolderPicker(_MoveImagesToFolderPath[0]);
            if (!pathCheck.Contains(rootFolder.FolderPath))
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Move Images", "You can only move images to folders that are within your root library.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
            //set the move to directory
            _mainWindowViewModel.SelectedImagesNewDirectory = PathHelper.FormatPathFromFolderPicker(_MoveImagesToFolderPath[0]);
        }
    }
}