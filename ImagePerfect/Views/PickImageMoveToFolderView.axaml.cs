using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ImagePerfect.ViewModels;
using ReactiveUI;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Avalonia.Platform.Storage;
using System.Linq;

namespace ImagePerfect.Views;

public partial class PickImageMoveToFolderView : ReactiveUserControl<PickImageMoveToFolderViewModel>
{
    public PickImageMoveToFolderView()
    {
        InitializeComponent();
        this.WhenActivated(d => d(ViewModel!.SelectMoveImagesToFolderInteration.RegisterHandler(InteractionHandler)));
    }

    private async Task InteractionHandler(IInteractionContext<string, List<string>?> context)
    {
        // Get our parent top level control in order to get the needed service (in our sample the storage provider. Can also be the clipboard etc.)
        TopLevel? topLevel = TopLevel.GetTopLevel(this);
        var startLocation = await topLevel!.StorageProvider.TryGetFolderFromPathAsync(context.Input);
        var storageFolder = await topLevel!.StorageProvider.OpenFolderPickerAsync(
                new FolderPickerOpenOptions()
                {
                    AllowMultiple = false,
                    Title = "Select New Folder",
                    SuggestedStartLocation = startLocation,
                }
            );
        context.SetOutput(storageFolder.Select(x => x.Path.ToString()).ToList());
    }
}