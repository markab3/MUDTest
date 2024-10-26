using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MUDTest
{
    class Program
    {
        static void Main(string[] args)
        {
            TestConnection[] connections = new TestConnection[25];
            for (int i = 0; i < 25; i++)
            {
                connections[i] = new TestConnection(i + 1);
            }

            foreach (TestConnection currentConnection in connections)
            {
                Task t = Task.Run(() =>
                    {
                        currentConnection.DoTest();
                    });
            }

            while (Console.ReadKey(true).Key != ConsoleKey.Q) { }

            for (int i = 0; i < 25; i++)
            {
                connections[i].IsTestRunning = false;
            }
        }
    }
}
