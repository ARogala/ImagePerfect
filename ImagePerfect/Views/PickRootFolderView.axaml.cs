using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using ImagePerfect.ViewModels;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImagePerfect.Views;

public partial class PickRootFolderView : ReactiveUserControl<PickRootFolderViewModel>
{
    public PickRootFolderView()
    {
        InitializeComponent();
        this.WhenActivated(d => d(ViewModel!.SelectFolderInteraction.RegisterHandler(this.InteractionHandler)));
    }

    private async Task InteractionHandler(IInteractionContext<string, List<string>?> context)
    {
        // Get our parent top level control in order to get the needed service (in our sample the storage provider. Can also be the clipboard etc.)
        TopLevel topLevel = TopLevel.GetTopLevel(this);

        var storageFolder = await topLevel!.StorageProvider.OpenFolderPickerAsync(
                new FolderPickerOpenOptions()
                {
                    AllowMultiple = false,
                    Title = context.Input
                }
            );

        context.SetOutput(storageFolder.Select(x => x.Path.ToString()).ToList());
    }
}