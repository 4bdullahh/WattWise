using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using server_side.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace client_side
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            byte[] key = new byte[32];
            byte[] iv = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(key);
                rng.GetBytes(iv);
            }
            var clientSocketPerThread = new ThreadLocal<DealerSocket>();

            using (var poller = new NetMQPoller())
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

                        // class here

                        messageToServer.Append(base64Key); //2
                        messageToServer.Append(base64Iv); //3
                        messageToServer.Append(hashJson);  //4
                        messageToServer.Append(base64EncryptedData); //5
                        client.SendMultipartMessage(messageToServer);

                        PrintFrames("Client Sending", messageToServer);

                        Thread.Sleep(3000);
                    }
                }, TaskCreationOptions.LongRunning);
                
                
                poller.RunAsync();

                Console.Read();
                poller.Stop();
            }
        }

        private static void PrintFrames(string operationType, NetMQMessage message)
        {
            for (int i = 0; i < message.FrameCount; i++)
            {
                Console.WriteLine("{0} Socket : Frame[{1}] = {2}", operationType, i,
                    message[i].ConvertToString());
            }
        }
    }
}