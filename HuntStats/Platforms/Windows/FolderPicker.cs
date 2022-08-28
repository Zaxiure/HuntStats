
using WindowsFolderPicker = Windows.Storage.Pickers.FolderPicker;

namespace HuntStats.Platforms.Windows;

public class FolderPicker : IFolderPicker
{
    public async Task<string> PickFolder()
    {
        var folderPicker = new WindowsFolderPicker();
        folderPicker.FileTypeFilter.Add("*");

        // Get the current window's HWND by passing in the Window object
        var hwnd = ((MauiWinUIWindow)App.Current.Windows[0].Handler.PlatformView).WindowHandle;
        // Associate the HWND with the file picker
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

        var result = await folderPicker.PickSingleFolderAsync();

        if (result == null)
        {
            return null;
        }

        return result.Path;
    }
}