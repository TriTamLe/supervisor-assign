using System.Net;
using System.Net.Sockets;
using System.Text;

public class Server

{
    private static int port = 1234;
    private static IPAddress localAddr = IPAddress.Parse("127.0.0.1");
    public static void Start()
    {
        TcpListener server = new TcpListener(localAddr, port);
        try
        {
            server.Start();

            while (true)
            {
                Console.WriteLine("Waiting for a connection... ");
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Connected!");
                HandleClient(client);
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine("SocketException: {0}", e);
        }
        finally
        {
            server.Stop();
        }
    }

    private static void HandleClient(TcpClient client)
    {
        try
        {

            NetworkStream stream = client.GetStream();

            string file1Path = "./officer_received.csv";
            ReceiveFile(stream, file1Path);
            client.Close();

        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception: {0}", ex);
        }
    }

    static void ReceiveFile(NetworkStream stream, string savePath)
    {
        // Read the file size from the client
        byte[] fileSizeBytes = new byte[4];
        stream.Read(fileSizeBytes, 0, fileSizeBytes.Length);
        int fileSize = BitConverter.ToInt32(fileSizeBytes, 0);
        Console.WriteLine($"Receiving file of size: {fileSize}");

        // Create a FileStream to write the file to disk
        using (FileStream fileStream = File.Create(savePath))
        {
            // Buffer to hold the received data
            byte[] buffer = new byte[1024];
            int bytesRead;

            // Read data from the network stream and write it to the file
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                fileStream.Write(buffer, 0, bytesRead);
            }
        }

        Console.WriteLine($"Received file and saved as: {savePath}");
    }
}