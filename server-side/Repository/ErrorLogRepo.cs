using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using server_side.Repository.Interface;
using server_side.Repository.Models;
using server_side.Services.Interface;

namespace server_side.Repository;

public class ErrorLogRepo : IErrorLogRepo
{
    private readonly IFolderPathServices folderPathServices;
    public ErrorLogRepo(IFolderPathServices folderPathServices)
    {
        this.folderPathServices = folderPathServices;   
    }

    public void LogError(ErrorLogMessage errorLogMessage)
    {
        try
        {
            string jsonFilePath = Path.Combine(folderPathServices.GetWattWiseFolderPath(), "server-side", "Data",
                "ErrorLog.json");

            string existingJson = File.ReadAllText(jsonFilePath);
            JArray errorLog = JArray.Parse(existingJson);

            JObject userDataObject = new JObject
            {
                { "Message", errorLogMessage.Message },
            };

            errorLog.Add(userDataObject);

            string updatedJson = JsonConvert.SerializeObject(errorLog, Formatting.Indented);
            File.WriteAllText(jsonFilePath, updatedJson);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error Writing to ErrorLog.json: " + ex.Message);
        }
       
        
    }
    
    
}