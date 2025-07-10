using System;
using System.Collections.Generic;
using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;
using ReactiveUI;

namespace ImagePerfect.ViewModels
{
	public class ModifyImageDataViewModel : ViewModelBase
	{
        private readonly IUnitOfWork _unitOfWork;
		private readonly ImageMethods _imageMethods;
        private readonly MainWindowViewModel _mainWindowViewModel;
        public ModifyImageDataViewModel(IUnitOfWork unitOfWork, MainWindowViewModel mainWindowViewModel) 
		{
            _unitOfWork = unitOfWork;
            _mainWindowViewModel = mainWindowViewModel;
            _imageMethods = new ImageMethods(_unitOfWork);
        }
	}
}