using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ChatProgram
{
    internal class Program
    {
        
        public static void Main0(string[] args)
        {
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);
            Console.WriteLine("Have endpoint: "+endpoint);


            string input=Console.ReadLine();
            if (input == "s")
            {
                TcpListener listener = new TcpListener(endpoint);
                Console.WriteLine("Listener: "+listener);
                listener.Start();
                Console.WriteLine("listener started");
                Task<TcpClient> clientTask = listener.AcceptTcpClientAsync();
                Console.WriteLine("client connected: "+clientTask.Result.Client.RemoteEndPoint);
                NetworkStream clientStream = clientTask.Result.GetStream();
                byte[] inputBuffer = new byte[1024];
                int receivedBytes = clientStream.Read(inputBuffer, 0, inputBuffer.Length);
                Console.WriteLine("Received: "+Encoding.ASCII.GetString(inputBuffer, 0, receivedBytes));
                Byte[] bytes = Encoding.ASCII.GetBytes("Hello World");
                clientStream.Write(bytes, 0, bytes.Length);
                clientStream.Flush();
                Console.WriteLine("client stream completed");
            }
            else
            {
                TcpClient tcpClient = new TcpClient();
                Console.WriteLine("Connecting");
                tcpClient.Connect(endpoint);
                Console.WriteLine("Connected");
                NetworkStream networkStream = tcpClient.GetStream();
                Byte[] bytes = Encoding.ASCII.GetBytes("Hi");
                networkStream.Write(bytes, 0, bytes.Length);
                networkStream.Flush();
                Console.WriteLine("Receiving message");
                byte[] inputBuffer = new byte[1024];
                int receivedBytes = networkStream.Read(inputBuffer, 0, inputBuffer.Length);
                // Task receivedBytesProcess = await networkStream.ReadAsync(inputBuffer, 0, inputBuffer.Length);
                Console.WriteLine("Received");
                Console.WriteLine("Received: " + Encoding.ASCII.GetString(inputBuffer, 0, receivedBytes));
            }
            
            
            
        }
    }
}