using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using System.Diagnostics;
using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;
using System.Collections.ObjectModel;
using ImagePerfect.Helpers;
using Avalonia;
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
        private bool _showLoading;
        private ObservableCollection<FolderViewModel> _libraryFolders;
           
        public PickRootFolderViewModel(IUnitOfWork unitOfWork, ObservableCollection<FolderViewModel> LibraryFolders) 
		{
            _unitOfWork = unitOfWork;
            _libraryFolders = LibraryFolders;
            _folderMethods = new FolderMethods(_unitOfWork);
            _folderCsvMethods = new FolderCsvMethods(_unitOfWork);
            _SelectFolderInteraction = new Interaction<string, List<string>?>();
            _showLoading = false;
            SelectLibraryFolderCommand = ReactiveCommand.CreateFromTask(SelectLibraryFolder);
        }

        private List<string>? _RootFolderPath;
        
        private readonly Interaction<string, List<string>?> _SelectFolderInteraction;

        public Interaction<string, List<string>?> SelectFolderInteraction { get { return _SelectFolderInteraction; } }

        public ReactiveCommand<Unit, Unit> SelectLibraryFolderCommand { get; }

        public bool ShowLoading
        {
            get => _showLoading;
            set => this.RaiseAndSetIfChanged(ref _showLoading, value);   
        }
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
            
            ShowLoading = true;
            //build csv
            bool csvIsSet = await FolderCsvMethods.BuildFolderTreeCsv(_RootFolderPath[0]);
            //write csv to database
            if (csvIsSet) 
            {
                await _folderCsvMethods.AddFolderCsv();

                rootFolder = await _folderMethods.GetRootFolder();
                if (rootFolder != null)
                {
                    FolderViewModel rootFolderVm = await FolderMapper.GetFolderVm(rootFolder);
                    _libraryFolders.Add(rootFolderVm);
                }
            }
            ShowLoading = false;
        }
    }
}