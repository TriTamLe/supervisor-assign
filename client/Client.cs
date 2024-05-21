using System;
using System.Net.Sockets;
using System.Text;

public class Client
{
    public static void Start()
    {
        try
        {
            TcpClient client = new TcpClient("127.0.0.1", 1234);

            NetworkStream stream = client.GetStream();
            Console.WriteLine("Connected to server.");
            Console.WriteLine("Sending file to server...");
            SendFile(stream, "./send-files/can-bo.csv");
            Console.WriteLine("File sent to server.");


            Console.WriteLine("Server has read all data of the file.");
            SendFile(stream, "./send-files/phong-thi.csv");
            Console.WriteLine("File sent to server.");

            byte[] fileSizeBytes1 = new byte[4];
            stream.Read(fileSizeBytes1, 0, fileSizeBytes1.Length);
            int fileSize1 = BitConverter.ToInt32(fileSizeBytes1, 0);
            ReceiveFile(stream, "./receive-files/giam-thi-phong-thi.csv", fileSize1);

            byte[] fileSizeBytes2 = new byte[4];
            stream.Read(fileSizeBytes2, 0, fileSizeBytes2.Length);
            int fileSize2 = BitConverter.ToInt32(fileSizeBytes2, 0);
            ReceiveFile(stream, "./receive-files/giam-sat-hanh-lang.csv", fileSize2);
            stream.Close();
            client.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception: {0}", ex);
        }
    }

    static void SendFile(NetworkStream stream, string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);

        byte[] fileSize = BitConverter.GetBytes(fileData.Length);
        stream.Write(fileSize, 0, fileSize.Length);

        stream.Write(fileData, 0, fileData.Length);
        Console.WriteLine($"Sent {Path.GetFileName(filePath)} to server.");
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



}
