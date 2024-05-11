using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

class TCPServer
{
    static void Main(string[] args)
    {
        TcpListener server = null;
        try
        {
            // Set the IP address and port number for the server
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            int port = 8888;

            // Initialize the TcpListener
            server = new TcpListener(ipAddress, port);

            // Start listening for client requests
            server.Start();

            Console.WriteLine("Server started...");

            while (true)
            {
                Console.WriteLine("Waiting for a connection...");

                // Accept client connection
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Connected!");

                // Get a stream object for reading and writing
                NetworkStream stream = client.GetStream();

                // Receive the first CSV file
                string file1Path = "file1_received.csv";
                ReceiveFile(stream, file1Path);

                // Receive the second CSV file
                string file2Path = "file2_received.csv";
                ReceiveFile(stream, file2Path);

                // Process the received files
                string mergedData = MergeCSVData(file1Path, file2Path);

                // Save the merged data to a new CSV file
                string mergedFilePath = "merged_data.csv";
                File.WriteAllText(mergedFilePath, mergedData);
                Console.WriteLine("Merged data saved as merged_data.csv");

                // Send the merged file back to client
                SendFile(stream, mergedFilePath);

                // Close connection
                client.Close();
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine("SocketException: {0}", e);
        }
        finally
        {
            // Stop listening for new clients
            server.Stop();
        }
    }

    static void ReceiveFile(NetworkStream stream, string savePath)
    {
        // Read the file size from the client
        byte[] fileSizeBytes = new byte[4];
        stream.Read(fileSizeBytes, 0, fileSizeBytes.Length);
        int fileSize = BitConverter.ToInt32(fileSizeBytes, 0);

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

    static string MergeCSVData(string file1Path, string file2Path)
    {
        // Open the files for reading
        using (StreamReader file1Reader = new StreamReader(file1Path))
        using (StreamReader file2Reader = new StreamReader(file2Path))
        using (StreamWriter mergedDataWriter = new StreamWriter("merged_data.csv"))
        {
            // Read the first line of each file (headers) and discard them
            file1Reader.ReadLine();
            file2Reader.ReadLine();

            string file1Line;
            string file2Line;

            Random rng = new Random();

            // Merge the data from both files
            while ((file1Line = file1Reader.ReadLine()) != null && (file2Line = file2Reader.ReadLine()) != null)
            {
                string[] file1Fields = file1Line.Split(',');
                string[] file2Fields = file2Line.Split(',');

                // Take one ID from file 1 and two IDs from file 2
                string mergedRow = $"{file1Fields[0]},{file2Fields[0]},{file2Fields[1]}";

                mergedDataWriter.WriteLine(mergedRow);
            }
        }

        return "merged_data.csv";
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
