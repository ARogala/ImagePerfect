using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using System.Diagnostics;
using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;

namespace ImagePerfect.ViewModels
{
	public class PickRootFolderViewModel : ViewModelBase
	{
        private readonly IUnitOfWork _unitOfWork;
        private readonly FolderCsvMethods _folderCsvMethods;
        public PickRootFolderViewModel(IUnitOfWork unitOfWork) 
		{
            _unitOfWork = unitOfWork;
            _folderCsvMethods = new FolderCsvMethods(_unitOfWork);
            _SelectFolderInteraction = new Interaction<string, List<string>?>();
            SelectLibraryFolderCommand = ReactiveCommand.CreateFromTask(SelectLibraryFolder);
        }

        private List<string>? _RootFolderPath;
        
        private readonly Interaction<string, List<string>?> _SelectFolderInteraction;

        public Interaction<string, List<string>?> SelectFolderInteraction { get { return _SelectFolderInteraction; } }

        public ReactiveCommand<Unit, Unit> SelectLibraryFolderCommand { get; }
        private async Task SelectLibraryFolder()
        {
            _RootFolderPath = await _SelectFolderInteraction.Handle("Select Root Library Folder");
            //build csv
            bool csvIsSet = await FolderCsvMethods.BuildFolderTreeCsv(_RootFolderPath[0]);
            //write csv to database
            if (csvIsSet) 
            {
               await _folderCsvMethods.AddFolderCsv();
            }
            //need a way to notify UI processing and finished etc..
        }
    }
}