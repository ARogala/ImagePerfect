using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using ImagePerfect.ObjectMappers;

namespace ImagePerfect.ViewModels
{
	public class PickRootFolderViewModel : ViewModelBase
	{
        private readonly IUnitOfWork _unitOfWork;
        private readonly FolderCsvMethods _folderCsvMethods;
        private readonly FolderMethods _folderMethods;
        private readonly MainWindowViewModel _mainWindowViewModel;
           
        public PickRootFolderViewModel(IUnitOfWork unitOfWork, MainWindowViewModel mainWindowViewModel) 
		{
            _unitOfWork = unitOfWork;
            _folderMethods = new FolderMethods(_unitOfWork);
            _folderCsvMethods = new FolderCsvMethods(_unitOfWork);
            _SelectFolderInteraction = new Interaction<string, List<string>?>();
            SelectLibraryFolderCommand = ReactiveCommand.CreateFromTask(SelectLibraryFolder);
            _mainWindowViewModel = mainWindowViewModel;
        }

        private List<string>? _RootFolderPath;
        
        private readonly Interaction<string, List<string>?> _SelectFolderInteraction;

        public Interaction<string, List<string>?> SelectFolderInteraction { get { return _SelectFolderInteraction; } }

        public ReactiveCommand<Unit, Unit> SelectLibraryFolderCommand { get; }

        private async Task SelectLibraryFolder()
        {
            Folder? rootFolder = await _folderMethods.GetRootFolder();
            if (rootFolder != null)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Add Library", "You already have a root library folder. You have to delete your library to add different one.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }

            _RootFolderPath = await _SelectFolderInteraction.Handle("Select Root Library Folder");
            //list will be empty if Cancel is pressed exit method
            if (_RootFolderPath.Count == 0) 
            {
                return;
            }
            
            _mainWindowViewModel.ShowLoading = true;
            //build csv
            bool csvIsSet = await FolderCsvMethods.AddNewFoldersCsv(_RootFolderPath, true);
            //write csv to database
            if (csvIsSet) 
            {
                await _folderCsvMethods.AddFolderCsv();

                rootFolder = await _folderMethods.GetRootFolder();
                if (rootFolder != null)
                {
                    FolderViewModel rootFolderVm = await FolderMapper.GetFolderVm(rootFolder);
                    _mainWindowViewModel.LibraryFolders.Add(rootFolderVm);
                }
            }
            _mainWindowViewModel.ShowLoading = false;
        }
    }
}