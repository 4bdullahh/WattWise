using System;
using System.Security.Cryptography;
using Newtonsoft.Json;
using NetMQ;
using NetMQ.Sockets;
using server_side.Cryptography;

namespace client_side
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] key = new byte[32];
            byte[] iv = new byte[16]; 
            
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(key);
                rng.GetBytes(iv);
            }
            
            using (var client = new RequestSocket())
            {
                client.Connect("tcp://localhost:5555");
                for (int i = 0; i < 10; i++)
                {

                    var userData = new UserModel { UserID = 101, Topic = "getId" };
                    
                    var jsonRequest = JsonConvert.SerializeObject(userData);
                    
                    string hashJson = Cryptography.GenerateHash(jsonRequest);
                    
                    byte[] encryptedData = Cryptography.Encrypt(jsonRequest, key, iv);
                    string base64EncryptedData = Convert.ToBase64String(encryptedData);

                    // We might use this later for Electron
                    //var topic = userData.Topic;

                    string base64Key = Convert.ToBase64String(key);
                    string base64Iv = Convert.ToBase64String(iv);

                    var messageToSend = $"{base64Key}:{base64Iv}:{hashJson}:{base64EncryptedData}";
                    client.SendFrame(messageToSend);
                    Console.WriteLine($"Sending UserData: {messageToSend}");
                    Thread.Sleep(500);

                    var message = client.ReceiveFrameString();
                    // We might need this for Electron
                    // var jsonResponse = JsonConvert.DeserializeObject<UserModel>(message);
                    Console.WriteLine($"Received: {message}");
                }
            }
        }
    }
}
