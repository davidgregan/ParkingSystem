using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ParkingSystem.Api.Models.Entities;
public class Garage
{
    public Garage()
    {
        ParkingSpaces = new HashSet<ParkingSpace>();
    }
    [Key]
    [DefaultValue("00000000-0000-0000-0000-000000000000")]
    public Guid GarageId { get; set; }
    
    public string Name { get; set; }

    public virtual ICollection<ParkingSpace> ParkingSpaces { get; set; }

    public List<ParkingSpace> FindParkingSpace(Vehicle toPark)
    {
        List<ParkingSpace> targetSpaces = new ();
        if (toPark.Type == VehicleType.Bus)
        {
            foreach (var space in 
                     ParkingSpaces.Where(p=>p.Type==ParkingSpaceType.Large && String.IsNullOrEmpty(p.LicensePlate)))
            {
                var emptySpaces = space.FindAdjacentSpaces(VehicleType.Bus);
                if (emptySpaces.Count() >= (int)VehicleType.Bus)
                {
                    targetSpaces = emptySpaces.Take((int)VehicleType.Bus).ToList();
                    break;
                }
            }
        }
        else
        {
            var targetSpace = ParkingSpaces
                .FirstOrDefault(p =>(int)p.Type >= (int)toPark.Type && String.IsNullOrEmpty(p.LicensePlate));
            if(targetSpace!=default) targetSpaces.Add(targetSpace);
        }
        return targetSpaces;
    }

    public bool Validate()
    {
        if (this.ParkingSpaces == default)
                throw new MissingDataException("Invalid garage configuration: Garage has no spaces.");
        ValidateRowAdjacency();
        ValidateFloorAdjacency();
        ValidateEmpty();
        ValidateSpaceType();
        return true;
    }

    private void ValidateEmpty()
    {
        if (ParkingSpaces.Any(p => p.GarageId==GarageId && !String.IsNullOrEmpty(p.LicensePlate)))
            throw new DataValidationException("Parking garage must be empty in order to save.");
    }

    private void ValidateSpaceType()
    {
        var validTypes = new List<int>()
            {(int)ParkingSpaceType.Compact, (int)ParkingSpaceType.Large, (int)ParkingSpaceType.Motorcycle};
        if(ParkingSpaces.ToList().Any(p=>!validTypes.Contains((int)p.Type)))
            throw new DataValidationException("Invalid parking space type");
    }
    private void ValidateRowAdjacency()
    {
        List<ParkingSpace> validated = new();
        foreach (var parkingSpace in ParkingSpaces)
        {
            if (!validated.Contains(parkingSpace))
            {
                var row = parkingSpace.FindAdjacentSpaces();
                var rowIterator = row.FirstOrDefault(p=>p.PreviousSpace==default);
                var rowNumbers = row.GroupBy(r => r.RowNumber).Select(g => g.Key).ToList();
                if (rowNumbers.Count > 1)
                    throw new ParkingSpaceAdjacencyException(
                        $"Parking space adjacency spans multiple rows on rows: {String.Join(", ", rowNumbers)}");
                while (rowIterator?.NextSpace!=default)
                {
                    if (rowIterator.NextSpace.SpaceNumber != rowIterator.SpaceNumber + 1)
                        throw new ParkingSpaceAdjacencyException
                            ($"Adjacent rows do not have consecutive space numbers on row {rowIterator.RowNumber}");
                    rowIterator = rowIterator.NextSpace;
                }
                validated.AddRange(row);
            }
        }
    }
    private void ValidateFloorAdjacency()
    {
        List<ParkingSpace> validated = new();
        foreach (var parkingSpace in ParkingSpaces)
        {
            if (!validated.Contains(parkingSpace))
            {
                var row = parkingSpace.FindAdjacentSpaces();
                var rowNumbers = row.GroupBy(r => r.FloorNumber).Select(g => g.Key).ToList();
                if (rowNumbers.Count > 1)
                    throw new ParkingSpaceAdjacencyException(
                        $"Parking space adjacency spans multiple floors on rows: {String.Join(", ", rowNumbers)}");
                validated.AddRange(row);
            }
        }
    }
}