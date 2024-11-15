using System;
using System.Collections.Generic;
using System.IO;
using server_side.Services;
using server_side.Services.Interface;

namespace server_side.Cryptography
{
    public class GenerateEnvFile
    {
        private readonly IFolderPathServices folderpath;

        public GenerateEnvFile(IFolderPathServices folderpath)
        {
            this.folderpath = folderpath;
        }
        /*
         * Class Documentation
             This class is responsible for handle Env files and include methods for:
                Generate/Load Env Files which store private and public key
         */
        public void EnvFileGenerator()
        {
            string envFilePath = Path.Combine(folderpath.GetWattWiseFolderPath(), "server-side", ".env");

            var envVariables = LoadExistingEnvVariables(envFilePath);

            if (!envVariables.ContainsKey("RSA_PRIVATE_KEY") || !envVariables.ContainsKey("RSA_PUBLIC_KEY"))
            {
                var (publicKey, privateKey) = Cryptography.GenerateRsaKeys();
                envVariables["RSA_PRIVATE_KEY"] = privateKey;
                envVariables["RSA_PUBLIC_KEY"] = publicKey;
            }

            WriteEnvVariablesToFile(envFilePath, envVariables);
        }

        public Dictionary<string, string> LoadExistingEnvVariables(string envFilePath)
        {
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
            return envVariables;
        }

        public void WriteEnvVariablesToFile(string envFilePath, Dictionary<string, string> envVariables)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(envFilePath))
                {
                    foreach (var entry in envVariables)
                    {
                        writer.WriteLine($"{entry.Key}={entry.Value}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to write to env file: {envFilePath}, error: {e.Message}");
                throw;
            }
        }
    }
}
