using Avalonia.Controls;
using ImagePerfect.ViewModels;
using System;
using System.Linq;

namespace ImagePerfect.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            //Cast will fail when you try to add tags there are list boxs within the list box..
            //Maybe not the best way but this works for the current use case
            try
            {
                foreach (var added in e.AddedItems.Cast<ImageViewModel>())
                    added.IsSelected = true;

                foreach (var removed in e.RemovedItems.Cast<ImageViewModel>())
                    removed.IsSelected = false;
            }
            catch (Exception ex) 
            {
                return;
            }
           
        }
    }
}