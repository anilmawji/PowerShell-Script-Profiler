﻿using System.Text;
using Windows.Storage.Pickers;
using Windows.Storage;

namespace ScriptProfiler.Utility;

public static class MauiFileHelper
{
    public static async Task<FileResult?> PickFileAsync(PickOptions options)
    {
        try
        {
            FileResult? result = await FilePicker.PickAsync(options);

            if (result == null)
            {
                return null;
            }
            if (FileHasValidExtension(result.FileName, options))
            {
                return result;
            }
            return null;
        }
        catch (TaskCanceledException)
        {
            return null;
        }
    }

    public static bool FileHasValidExtension(string fileName, PickOptions options)
    {
        if (options.FileTypes == null)
        {
            return false;
        }
        foreach (string fileType in options.FileTypes.Value)
        {
            if (fileName.EndsWith(fileType, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    // TODO: Support macOS
    public static async ValueTask PickSaveFileAsync(string filename, Stream stream)
    {
        string extension = Path.GetExtension(filename);
        FileSavePicker fileSavePicker = new()
        {
            SuggestedFileName = filename
        };
        fileSavePicker.FileTypeChoices.Add(extension, [extension]);

        if (MauiWinUIApplication.Current?.Application?.Windows[0]?.Handler?.PlatformView is MauiWinUIWindow window)
        {
            WinRT.Interop.InitializeWithWindow.Initialize(fileSavePicker, window.WindowHandle);
        }

        StorageFile result = await fileSavePicker.PickSaveFileAsync();

        if (result != null)
        {
            using Stream fileStream = await result.OpenStreamForWriteAsync();
            fileStream.SetLength(0); // override
            await stream.CopyToAsync(fileStream);
        }
    }

    public static ValueTask PickSaveFileAsync(string filename, string fileContent)
    {
        MemoryStream stream = new(Encoding.Default.GetBytes(fileContent));

        return PickSaveFileAsync(filename, stream);
    }
}
