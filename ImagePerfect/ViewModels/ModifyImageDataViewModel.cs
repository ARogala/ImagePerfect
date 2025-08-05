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
using Avalonia.Controls;
using Image = ImagePerfect.Models.Image;
using System.Threading.Tasks;

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
        public async Task UpdateImage(ImageViewModel imageVm, string fieldUpdated)
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
                await ImageMetaDataHelper.AddRatingToImage(image);
            }
        }

        //remove the tag from the image_tag_join table 
        //Also need to remove imageMetaData
        public async Task EditImageTag(ImageViewModel imageVm)
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
        public async Task AddImageTag(ImageViewModel imageVm)
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

        public async Task AddMultipleImageTags(ListBox selectedTagsListBox)
        {
            if (selectedTagsListBox.DataContext != null && selectedTagsListBox.SelectedItems != null)
            {
                ImageViewModel imageVm = (ImageViewModel)selectedTagsListBox.DataContext;
                List<Tag> tagsToAdd = new List<Tag>();
                //nothing selected just return
                if (selectedTagsListBox.SelectedItems.Count == 0)
                {
                    return;
                }
                //if no current tags just add all to list
                if (imageVm.ImageTags == "" || imageVm.ImageTags == null)
                {
                    foreach (Tag selectedTag in selectedTagsListBox.SelectedItems)
                    {
                        tagsToAdd.Add(selectedTag);
                    }
                }
                //else only add non duplicates
                else
                {
                    foreach (Tag selectedTag in selectedTagsListBox.SelectedItems)
                    {
                        if (!imageVm.ImageTags.Contains(selectedTag.TagName))
                        {
                            tagsToAdd.Add(selectedTag);
                        }
                    }
                }
                //add new tags to ImageTags -- KEEP!! THIS IS NEEDED TO WRITE METADATA
                foreach (Tag selectedTag in tagsToAdd)
                {
                    if (string.IsNullOrEmpty(imageVm.ImageTags))
                    {
                        imageVm.ImageTags = selectedTag.TagName;
                    }
                    else
                    {
                        imageVm.ImageTags = imageVm.ImageTags + "," + selectedTag.TagName;
                    }
                }
                //build sql for bulk insert
                string sql = SqlStringBuilder.BuildSqlForAddMultipleImageTags(tagsToAdd, imageVm);
                //update sql db
                bool success = await _imageMethods.AddMultipleImageTags(sql);
                //write new tags to image file
                if (success)
                {
                    //write new tags to image metadata
                    await ImageMetaDataHelper.WriteTagToImage(imageVm);
                }
                else
                {
                    List<string> imageTags = imageVm.ImageTags.Split(",").ToList();
                    //if fail remove the tags from the Tags list in the UI
                    foreach (Tag tag in tagsToAdd)
                    {
                        imageTags.Remove(tag.TagName);
                    }
                    for (int i = 0; i < imageTags.Count; i++)
                    {
                        if (i == 0)
                        {
                            imageVm.ImageTags = imageTags[i];
                        }
                        else
                        {
                            imageVm.ImageTags = imageVm.ImageTags + "," + imageTags[i];

                        }
                    }
                }
            }
        }
    }
}