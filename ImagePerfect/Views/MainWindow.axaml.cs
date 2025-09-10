using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using ImagePerfect.Models;
using ImagePerfect.ViewModels;
using System.Linq;

namespace ImagePerfect.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectAllButton_Click(object? sender, RoutedEventArgs e)
        {

            // check if any item is currently unselected
            bool anyUnselected = ImagesListBox.Items
                                  .OfType<ImageViewModel>()
                                  .Any(i => !i.IsSelected);

            if (anyUnselected)
            {
                // select all
                ImagesListBox.SelectedItems.Clear();
                foreach (ImageViewModel item in ImagesListBox.Items.OfType<ImageViewModel>())
                {
                    ImagesListBox.SelectedItems.Add(item);
                    item.IsSelected = true;
                }
            }
            else
            {
                // unselect all
                foreach (ImageViewModel item in ImagesListBox.Items.OfType<ImageViewModel>())
                {
                    ImagesListBox.SelectedItems.Remove(item);
                    item.IsSelected = false;
                }
            }   
        }

        //keeps Vms IsSelected in sync with ListBox Selection
        private void ListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            foreach (var added in e.AddedItems.OfType<ImageViewModel>())
                added.IsSelected = true;

            foreach (var removed in e.RemovedItems.OfType<ImageViewModel>())
                removed.IsSelected = false;
        }

        private void Image_Rating_Zero(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.FindAncestorOfType<ListBoxItem>()?.DataContext is ImageViewModel imgVm)
            {
                imgVm.ImageRating = 0;
            }
        }
        //Sets Vms ImageRating to correct Star number before add rating command is called.
        private void Star_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is StarItem star &&
                btn.FindAncestorOfType<ListBoxItem>()?.DataContext is ImageViewModel imgVm)
            {
                
                imgVm.ImageRating = star.Number;
                // Command will execute via binding in XAML
            }
        }

        private void Folder_Rating_Zero(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.FindAncestorOfType<StackPanel>()?.DataContext is FolderViewModel folderVm)
            {
                folderVm.FolderRating = 0;
            }
        }
        private void Folder_Star_Click(object? sender, RoutedEventArgs e)
        {
            if(sender is Button btn && btn.DataContext is StarItem star &&
                btn.FindAncestorOfType<StackPanel>()?.DataContext is FolderViewModel folderVm)
            {
                folderVm.FolderRating = star.Number;    
            }
        }

    }
}