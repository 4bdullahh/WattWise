﻿using server_side.Services;

namespace server_side.Cryptography;

public class GenerateEnvFile
{
    private FolderPathServices folderpath;

    public void EnvFileGenerator()
    {
        
    folderpath = new FolderPathServices();
    string envFilePath = Path.Combine(folderpath.GetWattWiseFolderPath(), "server-side", ".env");
    string privateKeyFilePath = Path.Combine(folderpath.GetWattWiseFolderPath(), "server-side", "private_key.xml");
    string publicKeyFilePath = Path.Combine(folderpath.GetWattWiseFolderPath(), "server-side", "public_key.xml");

    var envVariables = new Dictionary<string, string>();

    if (File.Exists(envFilePath))
    {
        try
        {
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
        catch (Exception e)
        {
            Console.WriteLine($"Failed to load env file: {envFilePath}, error: {e.Message}");
            throw;
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
        try
        {
            foreach (var entry in envVariables)
            {
                writer.WriteLine($"{entry.Key}={entry.Value}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"We could not write env file: {envFilePath}, error: {e.Message}");
            throw;
        }
    }
}
}