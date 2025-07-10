using System;
using System.Collections.Generic;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System.Diagnostics;
using System.IO;
using ReactiveUI;
using ImagePerfect.Helpers;

namespace ImagePerfect.ViewModels
{
	public class ExternalProgramViewModel : ViewModelBase
	{
        private readonly MainWindowViewModel _mainWindowViewModel;
        public ExternalProgramViewModel(MainWindowViewModel mainWindowViewModel) 
        {
            _mainWindowViewModel = mainWindowViewModel;
        }
        public async void OpenImageInExternalViewer(ImageViewModel imageVm)
        {
            string externalImageViewerExePath = PathHelper.GetExternalImageViewerExePath();
            string imagePathForProcessStart = PathHelper.FormatImageFilePathForProcessStart(imageVm.ImagePath);
            if (File.Exists(imageVm.ImagePath) && File.Exists(externalImageViewerExePath))
            {
                Process.Start(externalImageViewerExePath, imagePathForProcessStart);
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Open Image", "You need to install nomacs.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
        }
        public async void OpenCurrentDirectoryWithExplorer()
        {
            string externalFileExplorerExePath = PathHelper.GetExternalFileExplorerExePath();
            string folderPathForProcessStart = PathHelper.FormatImageFilePathForProcessStart(_mainWindowViewModel.CurrentDirectory); //not an image path but all this did was wrap it in quotes
            if (File.Exists(externalFileExplorerExePath) && Directory.Exists(_mainWindowViewModel.CurrentDirectory))
            {
                Process.Start(externalFileExplorerExePath, folderPathForProcessStart);
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Open Folder", "Sorry something went wrong.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
        }
    }
}