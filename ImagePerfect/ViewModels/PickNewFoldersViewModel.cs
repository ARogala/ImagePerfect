using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;

namespace ImagePerfect.ViewModels
{
	public class PickNewFoldersViewModel : ViewModelBase
	{
		public PickNewFoldersViewModel() 
		{ 
			_SelectNewFoldersInteraction = new Interaction<string, List<string>?>();
			SelectNewFoldersCommand = ReactiveCommand.CreateFromTask(SelectNewFolders);
		}

		private List<string>? _NewFolders;

		private readonly Interaction<string, List<string>?> _SelectNewFoldersInteraction;

        public Interaction<string, List<string>?> SelectNewFoldersInteraction { get { return _SelectNewFoldersInteraction; } }

		public ReactiveCommand<Unit, Unit> SelectNewFoldersCommand { get; }

		private async Task SelectNewFolders()
		{
			_NewFolders = await _SelectNewFoldersInteraction.Handle("Select New Folders");
			//list will be empty if Cancel is pressed exit method
			if (_NewFolders.Count == 0) 
			{
				return;
			}
			foreach (string folder in _NewFolders) 
			{ 
				Debug.WriteLine(folder);	
			}
        }
    }
}