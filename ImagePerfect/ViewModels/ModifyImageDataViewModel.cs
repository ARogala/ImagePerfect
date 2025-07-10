using System;
using System.Collections.Generic;
using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using ReactiveUI;
using ImagePerfect.ObjectMappers;
using ImagePerfect.Helpers;
using System.Linq;

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

        //update image sql and metadata only. 
        public async void UpdateImage(ImageViewModel imageVm, string fieldUpdated)
        {
            Image image = ImageMapper.GetImageFromVm(imageVm);
            bool success = await _imageMethods.UpdateImage(image);
            if (!success)
            {
                var box = MessageBoxManager.GetMessageBoxStandard($"Add {fieldUpdated}", $"Image {fieldUpdated} update error. Try again.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
            //write rating to image metadata
            if (fieldUpdated == "Rating")
            {
                ImageMetaDataHelper.AddRatingToImage(image);
            }
        }

        //remove the tag from the image_tag_join table 
        //Also need to remove imageMetaData
        public async void EditImageTag(ImageViewModel imageVm)
        {
            if (imageVm.ImageTags == null || imageVm.ImageTags == "")
            {
                if (imageVm.Tags.Count == 1)
                {
                    await _imageMethods.DeleteImageTag(imageVm.Tags[0]);
                    //remove tag from image metadata
                    await ImageMetaDataHelper.WriteTagToImage(imageVm);
                }
                else if (imageVm.Tags.Count == 0)
                {
                    return;
                }
            }
            List<string> imageTags = imageVm.ImageTags.Split(",").ToList();
            ImageTag tagToRemove = null;
            foreach (ImageTag tag in imageVm.Tags)
            {
                if (!imageTags.Contains(tag.TagName))
                {
                    tagToRemove = tag;
                }
            }
            if (tagToRemove != null)
            {
                await _imageMethods.DeleteImageTag(tagToRemove);
                //remove tag from image metadata
                await ImageMetaDataHelper.WriteTagToImage(imageVm);
            }
        }
    }
}