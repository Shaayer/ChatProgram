using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MrV {
  class WebserverTest {
    public static void Main(string[] args) {
      IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);
      string timestampServerStart = GetHttpTimeStampString(DateTime.UtcNow);
      TcpListener listener = new TcpListener(endpoint);
      listener.Start();
      byte[] inputBuffer = new byte[512];
      while (!UserWantsToQuit()) {
        Task<TcpClient> clientSocketTask = listener.AcceptTcpClientAsync();
        DateTime waitStart = DateTime.UtcNow;
        while (!clientSocketTask.IsCompleted) {
          Log($"waiting ... {(DateTime.UtcNow - waitStart).Seconds}\r", ConsoleColor.Yellow);
          Thread.Sleep(1);
          if (UserWantsToQuit()) { return; }
        }
        LogLine("\nclient connected: " + clientSocketTask.Result.Client.RemoteEndPoint, ConsoleColor.Green);
        NetworkStream clientStream = clientSocketTask.Result.GetStream();
        int bytesReceived = 0;
        List<string> inputChunks = new List<string>();
        while (clientStream.DataAvailable) {
          int bytesInThisChunk = clientStream.Read(inputBuffer, 0, inputBuffer.Length);
          bytesReceived += bytesInThisChunk;
          string inputTextChunk = Encoding.ASCII.GetString(inputBuffer, 0, bytesInThisChunk);
          inputChunks.Add(inputTextChunk);
          Log($"\nreceiving {bytesInThisChunk}\r", ConsoleColor.Yellow);
          Thread.Sleep(1);
          if (UserWantsToQuit()) { return; }
        }
        LogLine($"\nreceived {bytesReceived} bytes:", ConsoleColor.Green);
        Console.Write(string.Join("", inputChunks));
        SendHtmlResponse(clientStream, "<b>hello</b> world!", timestampServerStart);
        clientStream.Close();
      }
    }

    public static string GetHttpTimeStampString(DateTime dateTime) {
      const string httpHeaderTimestampFormatUTC = "ddd, dd MMM yyyy HH:mm:ss";
      return dateTime.ToString(httpHeaderTimestampFormatUTC) + " GMT";
    }

    public static bool UserWantsToQuit() {
      char keyPress = GetCharNonBlocking();
      return (keyPress == 27 || keyPress == 'q');
    }

    public static char GetCharNonBlocking() {
      if (!Console.KeyAvailable) {
        return (char)0;
      }
      return Console.ReadKey(true).KeyChar;
    }

    public static void LogLine(string message, ConsoleColor color) {
      Log(message, color);
      Console.WriteLine();
    }

    public static void Log(string message, ConsoleColor color) {
      Console.ForegroundColor = color;
      Console.Write(message);
      Console.ResetColor();
    }

    public static void SendHtmlResponse(NetworkStream clientStream, string html, string serverLastModified) {
      const string serverPlatform = "Apache/2.4.4 (Win32) OpenSSL/0.9.8y PHP/5.4.16";
      string timestampNow = GetHttpTimeStampString(DateTime.UtcNow);
      string[] httpHeader = {
        "HTTP/1.1 200 OK", // https://developer.mozilla.org/en-US/docs/Web/HTTP/Status
        $"Date: {timestampNow}",
        $"Server: {serverPlatform}",
        $"Last-Modified: {serverLastModified}",
        $"ETag: \"{timestampNow.GetHashCode().ToString("x")}\"",
        "Accept-Ranges: bytes",
        $"Content-Length: {html.Length}",
        "Keep-Alive: timeout=5, max=100",
        "Connection: Keep-Alive",
        "Content-Type: text/html",
      };
      const string lineEnd = "\r\n";
      string htmlResponse = string.Join(lineEnd, httpHeader) + lineEnd + lineEnd + html;
      byte[] bytes = Encoding.ASCII.GetBytes(htmlResponse);
      clientStream.Write(bytes, 0, bytes.Length);
      clientStream.Flush();
      Console.WriteLine(htmlResponse);
    }
  }
}