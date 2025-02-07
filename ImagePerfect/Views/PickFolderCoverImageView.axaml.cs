using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using ImagePerfect.ViewModels;
using ReactiveUI;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;


namespace ImagePerfect.Views;

public partial class PickFolderCoverImageView : ReactiveUserControl<PickFolderCoverImageViewModel>
{
    public PickFolderCoverImageView()
    {
        InitializeComponent();
        this.WhenActivated(d => d(ViewModel!.SelectCoverImageInteraction.RegisterHandler(this.InteractionHandler)));
    }

    private async Task InteractionHandler(IInteractionContext<string, List<string>?> context)
    {
        // Get our parent top level control in order to get the needed service (in our sample the storage provider. Can also be the clipboard etc.)
        TopLevel? topLevel = TopLevel.GetTopLevel(this);
        var storageFile = await topLevel!.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions()
            {
                AllowMultiple = false,
                Title = context.Input
            }
        );

        context.SetOutput(storageFile.Select(x => x.Path.ToString()).ToList());
    }
}