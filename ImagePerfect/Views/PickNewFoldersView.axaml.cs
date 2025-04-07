using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using ImagePerfect.ViewModels;
using ReactiveUI;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace ImagePerfect.Views;

public partial class PickNewFoldersView : ReactiveUserControl<PickNewFoldersViewModel>
{
    public PickNewFoldersView()
    {
        InitializeComponent();
        this.WhenActivated(d => d(ViewModel!.SelectNewFoldersInteraction.RegisterHandler(this.InteractionHandler)));
    }

    private async Task InteractionHandler(IInteractionContext<string, List<string>?> context)
    {
        // Get our parent top level control in order to get the needed service (in our sample the storage provider. Can also be the clipboard etc.)
        TopLevel? topLevel = TopLevel.GetTopLevel(this);
        var startLocation = await topLevel!.StorageProvider.TryGetFolderFromPathAsync(context.Input);
        var storageFolders = await topLevel!.StorageProvider.OpenFolderPickerAsync(
                new FolderPickerOpenOptions()
                {
                    AllowMultiple = true,
                    Title = "Select New Folders",
                    SuggestedStartLocation = startLocation
                }
            );
        context.SetOutput(storageFolders.Select(x => x.Path.ToString()).ToList());
    }
}