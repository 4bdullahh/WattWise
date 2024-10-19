﻿using server_side.Services.Interface;

namespace server_side.Services
{
    public class FolderPathServices : IFolderPathServices
    {
        public string GetServerSideFolderPath()
        {
            string folderName = "server-side";
            var currentDirectory = new DirectoryInfo(Environment.CurrentDirectory);
            while (currentDirectory != null && currentDirectory.Name != folderName)
            {
                currentDirectory = currentDirectory.Parent;
            }
            if (currentDirectory == null)
            {
                throw new DirectoryNotFoundException($"Could not find the '{folderName}' directory.");
            }
            return currentDirectory.FullName;
        }
        public string GetWattWiseFolderPath()
        {
            string folderName = "WattWise";  
            var currentDirectory = new DirectoryInfo(Environment.CurrentDirectory);

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
    
}

