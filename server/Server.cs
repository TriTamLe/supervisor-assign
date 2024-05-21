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


            Console.WriteLine("Waiting for a connection... ");
            TcpClient client = server.AcceptTcpClient();
            Console.WriteLine("Connected!");
            HandleClient(client);

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
            // Read the file size from the client
            byte[] fileSizeBytes1 = new byte[4];
            stream.Read(fileSizeBytes1, 0, fileSizeBytes1.Length);
            int fileSize1 = BitConverter.ToInt32(fileSizeBytes1, 0);
            ReceiveFile(stream, file1Path, fileSize1);


            string file2Path = "./room_received.csv";
            // Read the file size from the client
            byte[] fileSizeBytes2 = new byte[4];
            stream.Read(fileSizeBytes2, 0, fileSizeBytes2.Length);
            int fileSize2 = BitConverter.ToInt32(fileSizeBytes2, 0);

            ReceiveFile(stream, file2Path, fileSize2);
            // Process the received files

            AssignHandler.Assign();

            SendFile(stream, "./assignments.csv");
            SendFile(stream, "./supervisor.csv");


            client.Close();

        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception: {0}", ex);
        }
    }

    static void ReceiveFile(NetworkStream stream, string savePath, int fileSize)
    {
        // Create a FileStream to write the file to disk
        using (FileStream fileStream = File.Create(savePath))
        {
            // Buffer to hold the received data
            byte[] buffer = new byte[1024];
            int bytesRead;
            int totalBytesRead = 0;

            // Read data from the network stream and write it to the file
            while (totalBytesRead < fileSize && (bytesRead = stream.Read(buffer, 0, Math.Min(buffer.Length, fileSize - totalBytesRead))) > 0)
            {
                fileStream.Write(buffer, 0, bytesRead);
                totalBytesRead += bytesRead;
            }
        }

        Console.WriteLine($"Received file and saved as: {savePath}");
    }

    static void SendFile(NetworkStream stream, string filePath)
    {
        // Read the file content into a byte array
        byte[] fileData = File.ReadAllBytes(filePath);

        // Send the file size to the client
        byte[] fileSize = BitConverter.GetBytes(fileData.Length);
        stream.Write(fileSize, 0, fileSize.Length);

        // Send the file data to the client
        stream.Write(fileData, 0, fileData.Length);
        Console.WriteLine($"Sent {Path.GetFileName(filePath)} to client.");
    }

}