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

        //keeps Vms IsSelected in sync with ListBox Selection
        private void ListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            foreach (var added in e.AddedItems.OfType<ImageViewModel>())
                added.IsSelected = true;

            foreach (var removed in e.RemovedItems.OfType<ImageViewModel>())
                removed.IsSelected = false;
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

    }
}