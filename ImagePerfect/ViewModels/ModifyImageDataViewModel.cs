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

        //update ImageTags in db, and update image metadata
        public async void AddImageTag(ImageViewModel imageVm)
        {
            //click submit with empty input just return
            if (imageVm.NewTag == "" || imageVm.NewTag == null)
            {
                return;
            }
            //add NewTag to ImageTags -- KEEP!! THIS IS NEEDED TO WRITE METADATA
            if (string.IsNullOrEmpty(imageVm.ImageTags))
            {
                imageVm.ImageTags = imageVm.NewTag;
            }
            else
            {
                imageVm.ImageTags = imageVm.ImageTags + "," + imageVm.NewTag;
            }
            Image image = ImageMapper.GetImageFromVm(imageVm);
            //update image table and tags table in db -- success will be false if you try to input a duplicate tag
            bool success = await _imageMethods.UpdateImageTags(image, imageVm.NewTag);
            if (success)
            {
                //write new tag to image metadata
                await ImageMetaDataHelper.WriteTagToImage(imageVm);
                //Update TagsList to show in UI AutoCompleteBox clear NewTag in box as well
                await _mainWindowViewModel.GetTagsList();
                imageVm.NewTag = "";
            }
            else
            {
                //remove the NewTag from the Tags list in the UI (New tag was duplicate and not added in this case)
                int tagsMaxIndex = imageVm.ImageTags.Length - 1;
                int newTagTotalCharsToRemove = imageVm.NewTag.Length; //total chars to remove
                int removeStartAtIndex = tagsMaxIndex - newTagTotalCharsToRemove;
                imageVm.ImageTags = imageVm.ImageTags.Remove(removeStartAtIndex);
                //clear NewTag in box if try to input duplicate tag
                imageVm.NewTag = "";
            }
        }
    }
}