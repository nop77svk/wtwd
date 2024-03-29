﻿namespace wtwd.Utilities;
using Microsoft.Win32.TaskScheduler;
using System.Runtime.InteropServices;

public static class WindowsSchedulerTaskFolderExt
{
    public static TaskFolder CreateFolderIfNotExists(this TaskFolder folder, string folderName)
    {
        TaskFolder result;

        try
        {
            result = folder.CreateFolder(folderName);
        }
        catch (COMException e)
        {
            if (!e.Message.StartsWith("Cannot create a file when that file already exists."))
            {
                throw new Exception($"Error creating scheduler folder {folderName} under path {folder.Path}", e);
            }

            result = folder.SubFolders[folderName];
        }

        return result;
    }
}
