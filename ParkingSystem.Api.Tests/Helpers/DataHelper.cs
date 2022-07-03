using System.Collections.Generic;
using ParkingSystem.Api.Models.Entities;

namespace ParkingGarage.Api.Tests.Helpers;

public static class DataHelper
{
    private static int _motorcycleSpaces = 5;
    private static int _smallSpaces = 5;
    private static int _largeSpaces = 10;
    public static Garage GetValidGarage(int floors, int rowsPerFloor, int spacesPerRow)
    {
        var toReturn = new Garage();
        toReturn.Name = "Test Garage";
        var spaces = new List<ParkingSpace>();
        toReturn.ParkingSpaces = spaces;
        for (int i = 0; i < floors; i++)
        {
            var floorSpaces = GetRows(rowsPerFloor, spacesPerRow);
            floorSpaces.ForEach(p =>
            {
                p.FloorNumber = i;
                toReturn.ParkingSpaces.Add(p);
            });
        }
        AssignTypes(spaces);
        toReturn.ParkingSpaces = spaces;
        return toReturn;
    }

    private static List<ParkingSpace> GetRows(int rows, int spacesPerRow)
    {
        var toReturn = new List<ParkingSpace>();
        ParkingSpace? previous = default;
        int previousRow = 0;
        for (int i = 0; i < rows*spacesPerRow; i++)
        {
            var space = new ParkingSpace();
            space.SpaceNumber = i;
            space.RowNumber = (i / spacesPerRow);

            if (space.RowNumber != previousRow)
            {
                previousRow = space.RowNumber;
                previous = default;
            }
            else
            {
                space.PreviousSpace = previous;
                previous = space;
            }
            toReturn.Add(space);
        }

        return toReturn;
    }

    private static void AssignTypes(List<ParkingSpace> spaces)
    {
        int repeatPattern = _smallSpaces + _smallSpaces + _largeSpaces;
        int position = 0;
        foreach (var space in spaces)
        {
            if (position < _motorcycleSpaces)
                space.Type = ParkingSpaceType.Motorcycle;
            else if (position < _motorcycleSpaces + _smallSpaces)
                space.Type = ParkingSpaceType.Compact;
            else if (position < repeatPattern)
                space.Type = ParkingSpaceType.Large;
            position++;
            if (position == repeatPattern) position = 0;

        }
    }
}