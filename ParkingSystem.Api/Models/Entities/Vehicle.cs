using System.ComponentModel.DataAnnotations;
namespace ParkingSystem.Api.Models.Entities;

public class Vehicle
{
    public Vehicle()
    {
        ParkingSpaces = new HashSet<ParkingSpace>();
    }
    [Key]
    public string LicensePlate { get; set; } = null!;
    public VehicleType Type { get; set; }

    public virtual ICollection<ParkingSpace> ParkingSpaces { get; set; }

    public bool Validate()
    {
        if (String.IsNullOrEmpty(LicensePlate))
            throw new DataValidationException("Invalid license plate on vehicle.");
        
        var validTypes = new List<int>()
            {(int)VehicleType.Compact, (int)VehicleType.Large, (int)VehicleType.Motorcycle, (int)VehicleType.Bus};
        if(!validTypes.Contains((int)Type))
            throw new DataValidationException($"Invalid vehicle type on {LicensePlate}");
        return true;
    }
}