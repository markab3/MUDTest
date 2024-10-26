using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class AsynchronousClient
{
    public const int _bufferSize = 1024;
    private byte[] _buffer = new byte[_bufferSize];
    // ManualResetEvent instances signal completion.  
    private ManualResetEvent connectDone = new ManualResetEvent(false);
    private ManualResetEvent sendDone = new ManualResetEvent(false);
    private ManualResetEvent receiveDone = new ManualResetEvent(false);
    private Socket _workSocket;
    private Encoding _clientEncoding = Encoding.ASCII;
    public void StartClient(string host, int port)
    {
        // Connect to a remote device.  
        try
        {
            // Establish the remote endpoint for the socket.  
            // The name of the
            // remote device is "host.contoso.com".  
            IPAddress ipAddress = null;

            if (!IPAddress.TryParse(host, out ipAddress))
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry("host.contoso.com");
                ipAddress = ipHostInfo.AddressList[0];
            }
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.  
            _workSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Connect to the remote endpoint.  
            _workSocket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), _workSocket); connectDone.WaitOne();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public void Disconnect()
    {
        if (_workSocket.Connected)
        {
            // Release the socket.  
            _workSocket.Shutdown(SocketShutdown.Both);
            _workSocket.Close();
        }
    }
    private void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket client = (Socket)ar.AsyncState;

            // Complete the connection.  
            client.EndConnect(ar);

            Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());

            // Signal that the connection has been made.  
            connectDone.Set();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public void Receive()
    {
        try
        {
            if (_workSocket != null && _workSocket.Connected)
            {
                _workSocket.BeginReceive(_buffer, 0, _bufferSize, 0, new AsyncCallback(ReceiveCallback), this);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            Disconnect();
        }
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            // Read data from the remote device.  
            int bytesRead = _workSocket.EndReceive(ar);

            if (bytesRead > 0)
            {
                // Grab raw data.
                byte[] receivedData = _buffer.Take(bytesRead).ToArray();

                // Decode.
                string receivedMessage = string.Empty;

                // Handle telnet commands.
                foreach (byte currentByte in receivedData)
                {
                    receivedMessage += _clientEncoding.GetString(new byte[] { currentByte }); // hmm...
                }
                Console.WriteLine(receivedMessage);

                Receive();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            Disconnect();
        }
    }

    public void Send(String data)
    {
        try
        {
            Console.WriteLine("Sending to server: " + data);

            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            _workSocket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), _workSocket);
        }
        catch (Exception ex)
        {

            Console.WriteLine(ex.ToString());
            Disconnect();
        }
    }

    private void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket client = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.  
            int bytesSent = client.EndSend(ar);

            // Signal that all bytes have been sent.  
            sendDone.Set();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            Disconnect();
        }
    }
}