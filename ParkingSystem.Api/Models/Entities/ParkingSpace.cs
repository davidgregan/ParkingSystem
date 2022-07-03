using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingSystem.Api.Models.Entities;
public class ParkingSpace
{
    [Key]
    [DefaultValue("00000000-0000-0000-0000-000000000000")]
    public Guid ParkingSpaceId { get; set; } 
    public Guid? NextSpaceId { get; set; }
    public Guid GarageId { get; set; }
    public string? LicensePlate { get; set; }
    public int FloorNumber { get; set; }
    public int RowNumber { get; set; }
    public int SpaceNumber { get; set; }
    public ParkingSpaceType Type { get; set; }

    [DefaultValue(null)]
    public virtual Garage? Garage { get; set; }
    public virtual Vehicle? Vehicle { get; set; }
    public virtual ParkingSpace? NextSpace { get; set; }
    public virtual ParkingSpace? PreviousSpace { get; set; }
    private enum ParseDirection { Previous, Next }
    /// <summary>
    /// Parse related spaces and return adjacent empty spaces up to a given size. If no size is specified,
    /// all spaces in the row are return regardless of whether they are occupied.
    /// </summary>
    /// <param name="size">Optional: VehicleType to find empty space for.</param>
    /// <returns></returns>
    public List<ParkingSpace> FindAdjacentSpaces(VehicleType size = default)
    {
        List<ParkingSpace> emptySpaces = new();
        var start = this;
        if (size!=default &&
            (!String.IsNullOrEmpty(LicensePlate) || this.Type !=ParkingSpaceType.Large)) return emptySpaces;
        emptySpaces.Add(this);
        NextSpace?.AddSpaces(emptySpaces,size,start,ParseDirection.Next);
        if (size == default || emptySpaces.Count < (int)size)
            PreviousSpace?.AddSpaces(emptySpaces,size,start,ParseDirection.Previous);
        return emptySpaces.OrderBy(p=>p.SpaceNumber).ToList();
    }

    private void AddSpaces(List<ParkingSpace> emptySpaces, VehicleType size, ParkingSpace start, ParseDirection direction)
    {
        if (this == start) 
            throw new ReferenceLoopException($"Circular reference detected in adjacent parking spaces in row {this.RowNumber}");
        if(size!=default &&
           (!String.IsNullOrEmpty(this.LicensePlate) || emptySpaces.Count==(int)size || this.Type<ParkingSpaceType.Large))
            return;
        emptySpaces.Add(this);
        if (direction == ParseDirection.Next)
            this.NextSpace?.AddSpaces(emptySpaces,size,start,direction);
        else if (direction == ParseDirection.Previous)
            this.PreviousSpace?.AddSpaces(emptySpaces,size,start,direction);
    }
}