using System;
using System.Collections.Generic;
using System.Diagnostics;
using ReactiveUI;

namespace ImagePerfect.ViewModels
{
	public class ToggleUIViewModel : ViewModelBase
	{
        private bool _showAllTags = false;
        public bool ShowAllTags
        {
            get => _showAllTags;
            set => this.RaiseAndSetIfChanged(ref _showAllTags, value);
        }

        public void ToggleListAllTags()
        {
            if (ShowAllTags)
            {
                ShowAllTags = false;
            }
            else
            {
                ShowAllTags = true;
            }
        }
    }
}