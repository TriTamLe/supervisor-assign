using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class Officer
{
    public string Order { get; set; }
    public string Guid { get; set; }
    public string FullName { get; set; }

    public Officer(string order, string guid, string fullName)
    {
        Order = order;
        Guid = guid;
        FullName = fullName;
    }
}
public class Room
{
    public string Order { get; set; }
    public string RoomID { get; set; }

    public Room(string order, string roomID)
    {
        Order = order;
        RoomID = roomID;
    }
}

public class Assignment
{
    public string RoomID { get; set; }
    public string Officer1Guid { get; set; }
    public string Officer1 { get; set; }
    public string Officer2Guid { get; set; }
    public string Officer2 { get; set; }

    public Assignment(string roomId, string officer1Guid, string officer1, string officer2Guid, string officer2)
    {
        RoomID = roomId;
        Officer1Guid = officer1Guid;
        Officer1 = officer1;
        Officer2Guid = officer2Guid;
        Officer2 = officer2;
    }

}

public class SupervisorAssignment
{
    public string SupervisorGuid { get; set; }
    public string SupervisorName { get; set; }
    public List<string> RoomIds { get; set; }

    public SupervisorAssignment(string supervisorGuid, string supervisorName, List<string> roomIds)
    {
        SupervisorGuid = supervisorGuid;
        SupervisorName = supervisorName;
        RoomIds = roomIds;
    }
}

class AssignHandler
{
    private static string OFFICER_FILE_PATH = "./received-files/officer_received.csv";
    private static string ROOM_FILE_PATH = "./received-files/room_received.csv";
    private static string ASSIGNMENTS_FILE_PATH = "./result-files//assignments.csv";
    private static string SUPERVISOR_FILE_PATH = "./result-files//supervisor.csv";
    private static string ASSIGNMENTS_TITLE = "PhongThi,MaCanBo1,TenCanBo1,MaCanBo2,TenCanBo2";
    private static string SUPERVISOR_TITLE = "MaCanBo,TenCanBo,PhongThi";

    public static void Assign()
    {
        List<Officer> officers = ReadOfficerCsvFile(OFFICER_FILE_PATH);
        List<Room> rooms = ReadRoomCsvFile(ROOM_FILE_PATH);

        var assignments = AssignOfficersToRooms(officers, rooms, out List<Officer> remainingOfficers);

        var supervisorAssignments = AssignSupervisorsToRooms(remainingOfficers, rooms);

        WriteCsvFile(ASSIGNMENTS_FILE_PATH, assignments);
        WriteSupervisorsCsvFile(SUPERVISOR_FILE_PATH, supervisorAssignments);

        Console.WriteLine("Assignments and Supervisor assignments have been written to " + ASSIGNMENTS_FILE_PATH + " and " + SUPERVISOR_FILE_PATH);
        Console.WriteLine("Assignments have been written to " + ASSIGNMENTS_FILE_PATH);
    }

    static List<Officer> ReadOfficerCsvFile(string filePath)
    {
        var officers = new List<Officer>();
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
                        officers.Add(new Officer(fields[0], fields[1], fields[2]));
                    }
                }
            }
        }
        return officers;
    }

    static List<Room> ReadRoomCsvFile(string filePath)
    {
        var rooms = new List<Room>();
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
                        rooms.Add(new Room(fields[0], fields[1]));
                    }
                }
            }
        }
        return rooms;
    }
    static List<Assignment> AssignOfficersToRooms(
    List<Officer> officers,
    List<Room> rooms,
    out List<Officer> remainingOfficers
    )
    {
        var assignments = new List<Assignment>();
        remainingOfficers = new List<Officer>();

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
            assignments.Add(new Assignment(roomId, officer1Guid, officer1, officer2Guid, officer2));
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        }

        while (officerIndex < officers.Count)
        {
            remainingOfficers.Add(officers[officerIndex]);
            officerIndex++;
        }

        return assignments;
    }

    static void WriteCsvFile(string filePath, List<Assignment> assignments)
    {
        using (var writer = new StreamWriter(filePath))
        {
            writer.WriteLine(ASSIGNMENTS_TITLE);
            foreach (var assignment in assignments)
            {
                writer.WriteLine($"{assignment.RoomID},{assignment.Officer1Guid},{assignment.Officer1},{assignment.Officer2Guid},{assignment.Officer2}");
            }
        }
    }

    static List<SupervisorAssignment> AssignSupervisorsToRooms(
        List<Officer> remainingOfficers,
        List<Room> rooms)
    {

        if (remainingOfficers.Count == 0)
        {
            throw new InvalidOperationException("No officers to assign as supervisors.");
        }

        rooms = rooms.Skip(1).ToList();

        var baseNumberOfRoom = rooms.Count / remainingOfficers.Count;

        var supervisorAssignments = new List<SupervisorAssignment>();

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

            supervisorAssignments.Add(new SupervisorAssignment(officer.Guid, officer.FullName, supervisedRooms));
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
    static void WriteSupervisorsCsvFile(string filePath, List<SupervisorAssignment> supervisorAssignments)
    {
        using (var writer = new StreamWriter(filePath))
        {
            writer.WriteLine(SUPERVISOR_TITLE);
            foreach (var assignment in supervisorAssignments)
            {
                writer.WriteLine($"{assignment.SupervisorGuid},{assignment.SupervisorName},{string.Join("; ", assignment.RoomIds)}");
            }
        }
    }
}
