using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;



class AssignHandler
{
    public static void Assign()
    {
        string officerFilePath = "./officer_received.csv";
        string roomFilePath = "./room_received.csv";
        string assignmentsFilePath = "./assignments.csv";
        string supervisorFilePath = "./supervisor.csv";

        List<(string Order, string Guid, string FullName)> officers = ReadOfficerCsvFile(officerFilePath);
        List<(string Order, string RoomID)> rooms = ReadRoomCsvFile(roomFilePath);

        var assignments = AssignOfficersToRooms(officers, rooms, out List<(string Order, string Guid, string FullName)> remainingOfficers);


        var supervisorAssignments = AssignSupervisorsToRooms(remainingOfficers, rooms);

        WriteCsvFile(assignmentsFilePath, assignments);
        WriteSupervisorsCsvFile(supervisorFilePath, supervisorAssignments);

        Console.WriteLine("Assignments and Supervisor assignments have been written to " + assignmentsFilePath + " and " + supervisorFilePath);


        Console.WriteLine("Assignments have been written to " + assignmentsFilePath);
    }

    static List<(string Order, string Guid, string FullName)> ReadOfficerCsvFile(string filePath)
    {
        var officers = new List<(string Order, string Guid, string FullName)>();
        using (var reader = new StreamReader(filePath))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    var fields = line.Split(',');
                    if (fields.Length >= 2)
                    {
                        officers.Add((fields[0], fields[1], fields[2]));
                    }
                }
            }
        }
        return officers;
    }

    static List<(string Order, string RoomID)> ReadRoomCsvFile(string filePath)
    {
        var rooms = new List<(string Order, string RoomID)>();
        using (var reader = new StreamReader(filePath))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    var fields = line.Split(',');
                    if (fields.Length >= 1)
                    {
                        rooms.Add((fields[0], fields[1]));
                    }
                }
            }
        }
        return rooms;
    }
    static List<(string RoomID, string Officer1Guid, string Officer1, string Officer2Guid, string Officer2)> AssignOfficersToRooms(
    List<(string Order, string Guid, string FullName)> officers,
    List<(string Order, string RoomID)> rooms,
    out List<(string Order, string Guid, string FullName)> remainingOfficers
    )
    {
        var assignments = new List<(string RoomID, string Officer1Guid, string Officer1, string Officer2Guid, string Officer2)>();
        remainingOfficers = new List<(string Order, string Guid, string FullName)>();

        rooms = rooms.Skip(1).ToList();
        officers = officers.Skip(1).ToList();

        Random random = new Random();
        rooms = rooms.OrderBy(x => random.Next()).ToList();
        officers = officers.OrderBy(x => random.Next()).ToList();

        int officerIndex = 0;
        foreach (var room in rooms)
        {
            if (officerIndex >= officers.Count)
            {
                throw new InvalidOperationException("Not enough officers to fill the rooms.");
            }

            string roomId = room.RoomID;
            string officer1Guid = officers[officerIndex].Guid;
            string officer1 = officers[officerIndex].FullName;
            officerIndex++;

            string? officer2Guid = officerIndex < officers.Count ? officers[officerIndex].Guid : null;
            string? officer2 = officerIndex < officers.Count ? officers[officerIndex].FullName : null;
            officerIndex++;



#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            assignments.Add((roomId, officer1Guid, officer1, officer2Guid, officer2));
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        }

        while (officerIndex < officers.Count)
        {
            remainingOfficers.Add(officers[officerIndex]);
            officerIndex++;
        }

        return assignments;
    }

    static void WriteCsvFile(string filePath, List<(string RoomID, string Officer1Guid, string Officer1, string Officer2Guid, string Officer2)> assignments)
    {
        using (var writer = new StreamWriter(filePath))
        {
            writer.WriteLine("RoomId,Officer1Guid,Officer1,Officer2Guid,Officer2");
            foreach (var assignment in assignments)
            {
                writer.WriteLine($"{assignment.RoomID},{assignment.Officer1Guid},{assignment.Officer1},{assignment.Officer2Guid},{assignment.Officer2}");
            }
        }
    }

    static List<(string SupervisorGuid, string SupervisorName, List<string> RoomIds)> AssignSupervisorsToRooms(
        List<(string Order, string Guid, string FullName)> remainingOfficers,
        List<(string Order, string RoomID)> rooms)
    {

        if (remainingOfficers.Count == 0)
        {
            throw new InvalidOperationException("No officers to assign as supervisors.");
        }

        rooms = rooms.Skip(1).ToList();

        var baseNumberOfRoom = rooms.Count / remainingOfficers.Count;

        var supervisorAssignments = new List<(string SupervisorGuid, string SupervisorName, List<string> RoomIds)>();

        int roomIndex = 0;
        foreach (var officer in remainingOfficers)
        {
            var supervisedRooms = new List<string>();

            // Ensure every supervisor has at least one room
            if (roomIndex < rooms.Count)
            {
                supervisedRooms.Add(rooms[roomIndex].RoomID);
                roomIndex++;
            }

            // Assign remaining rooms in a round-robin fashion
            while (roomIndex < rooms.Count && supervisedRooms.Count < baseNumberOfRoom)
            {
                supervisedRooms.Add(rooms[roomIndex].RoomID);
                roomIndex++;
            }

            supervisorAssignments.Add((officer.Guid, officer.FullName, supervisedRooms));
        }

        // Ensure all rooms are supervised
        int supervisorIndex = 0;
        while (roomIndex < rooms.Count)
        {
            supervisorAssignments[supervisorIndex].RoomIds.Add(rooms[roomIndex].RoomID);
            roomIndex++;
            supervisorIndex = (supervisorIndex + 1) % remainingOfficers.Count;
        }

        return supervisorAssignments;
    }
    static void WriteSupervisorsCsvFile(string filePath, List<(string SupervisorGuid, string SupervisorName, List<string> RoomIds)> supervisorAssignments)
    {
        using (var writer = new StreamWriter(filePath))
        {
            writer.WriteLine("SupervisorGuid,SupervisorName,RoomIds");
            foreach (var assignment in supervisorAssignments)
            {
                writer.WriteLine($"{assignment.SupervisorGuid},{assignment.SupervisorName},{string.Join(";", assignment.RoomIds)}");
            }
        }
    }
}
