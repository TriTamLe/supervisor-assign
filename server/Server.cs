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
            string mergedData = MergeCSVData(file1Path, file2Path);

            // Save the merged data to a new CSV file
            string mergedFilePath = "merged_data.csv";
            File.WriteAllText(mergedFilePath, mergedData);
            Console.WriteLine("Merged data saved as merged_data.csv");


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
}