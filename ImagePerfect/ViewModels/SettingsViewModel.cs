using System;
using System.Collections.Generic;
using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;
using ReactiveUI;

namespace ImagePerfect.ViewModels
{
	public class SettingsViewModel : ViewModelBase
	{
        private readonly IUnitOfWork _unitOfWork;
        
        private readonly SettingsMethods _setttingsMethods;
        private readonly MainWindowViewModel _mainWindowViewModel;
        public SettingsViewModel(IUnitOfWork unitOfWork, MainWindowViewModel mainWindowViewModel) 
		{
            _unitOfWork = unitOfWork;
            _mainWindowViewModel = mainWindowViewModel;
            _setttingsMethods = new SettingsMethods(_unitOfWork);
        }
	}
}