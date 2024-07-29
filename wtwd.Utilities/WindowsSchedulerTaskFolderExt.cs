namespace wtwd.Utilities;

using System.Runtime.InteropServices;

using Microsoft.Win32.TaskScheduler;

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
                throw new CannotCreateSubfolderException(folder.Path, folderName, e);
            }

            result = folder.SubFolders[folderName];
        }

        return result;
    }
}
