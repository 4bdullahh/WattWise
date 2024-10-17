using server_side.Repository.Interface;

namespace server_side.Repository;

public class WattWiseFolderPath : IWattWiseFolderPath
{
    public WattWiseFolderPath()
    {
    }
    
    public string GetWattWiseFolderPath()
    {
        string folderName = "WattWise";  // Set to find the 'WattWise' folder
        var currentDirectory = new DirectoryInfo(Environment.CurrentDirectory);

        // Traverse up the directory structure to find the WattWise folder
        while (currentDirectory != null && currentDirectory.Name != folderName)
        {
            currentDirectory = currentDirectory.Parent;
        }

        if (currentDirectory == null)
        {
            throw new DirectoryNotFoundException($"Could not find the '{folderName}' directory.");
        }

        return currentDirectory.FullName; // Return the full path to the WattWise folder
    }
}