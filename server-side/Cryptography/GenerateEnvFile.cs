namespace server_side.Cryptography;

public class GenerateEnvFile
{
    public void EnvFileGenerator()
    { 
        
    string envFilePath = Path.Combine(GetWattWiseFolderPath(), "server-side", ".env");
    string privateKeyFilePath = Path.Combine(GetWattWiseFolderPath(), "server-side", "private_key.xml");
    string publicKeyFilePath = Path.Combine(GetWattWiseFolderPath(), "server-side", "public_key.xml");

    var envVariables = new Dictionary<string, string>();

    if (File.Exists(envFilePath))
    {
        // Read the .env file
        string[] envLines = File.ReadAllLines(envFilePath);
        foreach (var line in envLines)
        {
            if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
            {
                var keyValue = line.Split('=', 2);
                if (keyValue.Length == 2)
                {
                    string key = keyValue[0].Trim();
                    string value = keyValue[1].Trim();
                    envVariables[key] = value;
                }
            }
        }
    }
    else
    {
        File.Create(envFilePath).Close();
    }

    if (File.Exists(privateKeyFilePath))
    {
        string privateKeyContent = File.ReadAllText(privateKeyFilePath);
        envVariables["RSA_PRIVATE_KEY"] = privateKeyContent;
    }
    if (File.Exists(publicKeyFilePath))
    {
        string publicKeyContent = File.ReadAllText(publicKeyFilePath);
        envVariables["RSA_PUBLIC_KEY"] = publicKeyContent;
    }
   
    using (StreamWriter writer = new StreamWriter(envFilePath))
    {
        foreach (var entry in envVariables)
        {
            writer.WriteLine($"{entry.Key}={entry.Value}");
        }
    }
}
    private string GetWattWiseFolderPath()
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
}