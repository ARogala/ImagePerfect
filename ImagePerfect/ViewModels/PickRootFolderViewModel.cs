using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using System.Diagnostics;
using ImagePerfect.Models;

namespace ImagePerfect.ViewModels
{
	public class PickRootFolderViewModel : ViewModelBase
	{
		public PickRootFolderViewModel() 
		{
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
            //pass root folder path to model method for processing
            //build csv
            bool csvIsSet = await FolderCsvMethods.BuildFolderTreeCsv(_RootFolderPath[0]);
            //write csv to database
        }
    }
}