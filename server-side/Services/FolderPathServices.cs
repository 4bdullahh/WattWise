using server_side.Services.Interface;

namespace server_side.Services
{
    public class FolderPathServices : IFolderPathServices
    {
        public string GetServerSideFolderPath()
        {
            try
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
            catch (Exception e)
            {
                Console.WriteLine($"We were unable to get the server side folder path: {e.Message}");
                throw;
            }
        }
        public string GetClientFolderPath()
        {
            try
            {
                string folderName = "client-side";
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
            catch (Exception e)
            {
                Console.WriteLine($"We were unable to get the client folder path: {e.Message}");
                throw;
            }
        }
        public string GetWattWiseFolderPath()
        {
            try
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

                return currentDirectory.FullName; 
            }
            catch (Exception e)
            {
                Console.WriteLine($"We were unable to get the wat Wise folder path: {e.Message}");
                throw;
            }
        }
    }
    
}

