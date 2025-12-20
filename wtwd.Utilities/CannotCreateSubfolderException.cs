namespace NoP77svk.wtwd.Utilities;

using System;

public class CannotCreateSubfolderException
    : Exception
{
    public string ParentPath { get; }
    public string FolderName { get; }

    public CannotCreateSubfolderException(string parentPath, string folderName)
        : base(GetErrorMessage(parentPath, folderName))
    {
        ParentPath = parentPath;
        FolderName = folderName;
    }

    public CannotCreateSubfolderException(string parentPath, string folderName, Exception? innerException)
        : base(GetErrorMessage(parentPath, folderName), innerException)
    {
        ParentPath = parentPath;
        FolderName = folderName;
    }

    private static string GetErrorMessage(string parentPath, string folderName)
    {
        return $"Error creating scheduler folder {folderName} under path {parentPath}";
    }
}
