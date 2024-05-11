﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using OfficeOpenXml;

class TCPClient
{
    static void Main(string[] args)
    {
        try
        {
            // Set the server IP address and port number
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            int port = 8888;

            // Initialize the TcpClient
            TcpClient client = new TcpClient();

            // Connect to the server
            client.Connect(ipAddress, port);

            Console.WriteLine("Connected to server...");

            // Get a stream object for reading and writing
            NetworkStream stream = client.GetStream();

            // Convert XLSX to CSV
            string xlsxFilePath = "input.xlsx";
            string csvFilePath = "output.csv";
            ConvertXlsxToCsv(xlsxFilePath, csvFilePath);

            // Send the CSV file to server
            SendFile(stream, csvFilePath);

            // Receive the merged CSV file from server
            string receivedCsvFilePath = "received_data.csv";
            ReceiveFile(stream, receivedCsvFilePath);

            // Convert the received CSV file back to XLSX
            string receivedXlsxFilePath = "received_data.xlsx";
            ConvertCsvToXlsx(receivedCsvFilePath, receivedXlsxFilePath);

            Console.WriteLine("Process completed. Received file converted to XLSX.");

            // Close the connection
            client.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: {0}", e);
        }
    }

    static void ConvertXlsxToCsv(string xlsxFilePath, string csvFilePath)
    {
        try
        {
            FileInfo xlsxFile = new FileInfo(xlsxFilePath);
            using (ExcelPackage package = new ExcelPackage(xlsxFile))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                using (StreamWriter writer = new StreamWriter(csvFilePath))
                {
                    for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
                    {
                        for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                        {
                            string cellValue = worksheet.Cells[row, col].Value?.ToString() ?? "";
                            writer.Write($"\"{cellValue}\"");

                            if (col < worksheet.Dimension.End.Column)
                            {
                                writer.Write(",");
                            }
                        }
                        writer.WriteLine();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while converting XLSX to CSV: {ex.Message}");
        }
    }

    static void ConvertCsvToXlsx(string csvFilePath, string xlsxFilePath)
    {
        try
        {
            FileInfo csvFile = new FileInfo(csvFilePath);
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");

                string[] lines = File.ReadAllLines(csvFilePath);
                for (int row = 0; row < lines.Length; row++)
                {
                    string[] fields = lines[row].Split(',');
                    for (int col = 0; col < fields.Length; col++)
                    {
                        worksheet.Cells[row + 1, col + 1].Value = fields[col];
                    }
                }

                package.SaveAs(new FileInfo(xlsxFilePath));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while converting CSV to XLSX: {ex.Message}");
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

    static void ReceiveFile(NetworkStream stream, string savePath)
    {
        byte[] fileSizeBytes = new byte[4];
        stream.Read(fileSizeBytes, 0, fileSizeBytes.Length);
        int fileSize = BitConverter.ToInt32(fileSizeBytes, 0);

        byte[] fileData = new byte[fileSize];
        int bytesRead = stream.Read(fileData, 0, fileSize);

        File.WriteAllBytes(savePath, fileData);
        Console.WriteLine($"Received file and saved as: {savePath}");
    }
}
