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
            SendFile(stream, "./officer.csv");
            Console.WriteLine("File sent to server.");


            Console.WriteLine("Server has read all data of the file.");
            SendFile(stream, "./room.csv");
            Console.WriteLine("File sent to server.");
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


}
