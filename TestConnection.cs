using System;

public class TestConnection
{
    public bool IsTestRunning = false;
    public int PlayerNum;

    public TestConnection(int playerNum)
    {
        PlayerNum = playerNum;
    }

    public void DoTest()
    {
        IsTestRunning = true;
        string[] directions = { "nw", "n", "ne", "w", "e", "sw", "s", "se" };
        string server = "192.168.0.200";
        int port = 7681;
        string message = "connect testplayer" + PlayerNum + " password";
        try
        {
            AsynchronousClient asyncClient = new AsynchronousClient();
            asyncClient.StartClient(server, port);
            asyncClient.Receive();
            asyncClient.Send(message + "\r");

            var rng = new Random();
            DateTime nextCommand = DateTime.Now.AddMilliseconds(1000);

            while (IsTestRunning)
            {
                if (DateTime.Now > nextCommand)
                {
                    message = directions[rng.Next(directions.Length)];
                    asyncClient.Send(message + "\r");
                    nextCommand = DateTime.Now.AddMilliseconds(1000);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: {0}", e);
        }
    }
}