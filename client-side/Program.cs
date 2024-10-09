using Newtonsoft.Json;
using NetMQ;
using NetMQ.Sockets;
using System.Text;


namespace client_side
{
    class Program
    {
        static void Main(string[] args)
        {
            var clientSocketPerThread = new ThreadLocal<DealerSocket>();
            using (var poller = new NetMQPoller())
            {

                Task.Factory.StartNew(state =>
                {
                    DealerSocket client = null;
                    if (!clientSocketPerThread.IsValueCreated)
                    {
                        client = new DealerSocket();
                        client.Connect("tcp://localhost:5555");
                        client.Options.Identity = Encoding.Unicode.GetBytes(state.ToString());


                        client.ReceiveReady += (s, e) =>
                        {
                            var response = e.Socket.ReceiveFrameStringAsync();
                            Console.WriteLine($"Response from server: {response}");

                        };

                        clientSocketPerThread.Value = client;
                        poller.Add(client);

                    }
                    else
                    {
                        client = clientSocketPerThread.Value;
                    }
                    while (true)
                    {
                        var messageToServer = new NetMQMessage();

                        messageToServer.Append(state.ToString());
                        messageToServer.AppendEmptyFrame();

                        var userData = new UserModel { UserID = 200, UserEmail = "fedhf@hotmail", Topic = "getId" };

                        switch (userData.Topic)
                        {
                            case "getId":
                                {
                                    var jsonRequest = JsonConvert.SerializeObject(userData);
                                    messageToServer.Append(jsonRequest);
                                }
                                break;
                            case "addUser":
                                {
                                    var jsonRequest = JsonConvert.SerializeObject(userData);
                                    messageToServer.Append(jsonRequest);
                                }
                                break;
                        }


                        client.SendMultipartMessage(messageToServer);

                        Thread.Sleep(500);
                    }
                }, TaskCreationOptions.LongRunning);

                poller.RunAsync();

                Console.WriteLine("Press any key to stop...");
                Console.Read();

                poller.Stop();
            }
        }
    }
}
