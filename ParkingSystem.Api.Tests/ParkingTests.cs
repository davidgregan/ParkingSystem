using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ParkingSystem.Api.Models;
using ParkingGarage.Api.Tests.Helpers;
using ParkingSystem.Api.Models.Entities;

namespace ParkingGarage.Api.Tests;
#nullable disable
public class ParkingTests
{
    private ParkingSystemDataContext _dataContext;
    [SetUp]
    public void Setup()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ParkingSystemDataContext>();
        optionsBuilder.UseLazyLoadingProxies();
        optionsBuilder.UseInMemoryDatabase("ParkingSystemTestDB");
        _dataContext = new ParkingSystemDataContext(optionsBuilder.Options);
    }
    [Test]
    public void ParkingTest_MotorCycle_ParkSuccess()
    {
        _dataContext.Database.EnsureDeleted();
        _dataContext.Database.EnsureCreated();
        _dataContext.Add(DataHelper.GetValidGarage(2, 2, 20));
        _dataContext.SaveChanges();
        var garage = _dataContext.Garages.First();
        var toPark = new Vehicle() {LicensePlate = "TestPlate", Type = VehicleType.Motorcycle};
        var spaces = garage.FindParkingSpace(toPark);
        if(spaces.Count==0) Assert.Fail("Space not found.");
        spaces.ForEach(s=>s.Vehicle = toPark);
        _dataContext.SaveChanges();
        toPark = _dataContext.Vehicles.FirstOrDefault();
        if(toPark==default) Assert.Fail("Vehicle Not Saved");
        if(toPark.ParkingSpaces.Count==0) Assert.Fail("Vehicle Not Parked");
        if((int)toPark.ParkingSpaces.First().Type!=(int)VehicleType.Motorcycle)
            Assert.Fail("Parked in wrong spot.");
        Assert.Pass("Successfully Parked");
    }
    [Test]
    public void ParkingTest_Compact_ParkSuccess()
    {
        _dataContext.Database.EnsureDeleted();
        _dataContext.Database.EnsureCreated();
        _dataContext.Add(DataHelper.GetValidGarage(2, 2, 20));
        _dataContext.SaveChanges();
        var garage = _dataContext.Garages.First();
        var toPark = new Vehicle() {LicensePlate = "TestPlate", Type = VehicleType.Compact};
        var spaces = garage.FindParkingSpace(toPark);
        if(spaces.Count==0) Assert.Fail("Space not found.");
        spaces.ForEach(s=>s.Vehicle = toPark);
        _dataContext.SaveChanges();
        toPark = _dataContext.Vehicles.FirstOrDefault();
        if(toPark==default) Assert.Fail("Vehicle Not Saved");
        if(toPark.ParkingSpaces.Count==0) Assert.Fail("Vehicle Not Parked");
        if(toPark.ParkingSpaces.First().Type!=ParkingSpaceType.Compact)
            Assert.Fail("Parked in wrong spot.");
        Assert.Pass("Successfully Parked");
    }
    [Test]
    public void ParkingTest_CompactInLargerSpace_ParkSuccess()
    {
        _dataContext.Database.EnsureDeleted();
        _dataContext.Database.EnsureCreated();
        var garage = DataHelper.GetValidGarage(2, 2, 10);
        garage.ParkingSpaces.ToList().ForEach(p=>p.Type=ParkingSpaceType.Large);
        _dataContext.Add(garage);
        _dataContext.SaveChanges();
        garage = _dataContext.Garages.First();
        var toPark = new Vehicle() {LicensePlate = "TestPlate", Type = VehicleType.Compact};
        var spaces = garage.FindParkingSpace(toPark);
        if(spaces.Count>0 ) Assert.Pass("Space found.");
        else Assert.Fail("No space found with larger space available");
    }
    [Test]
    public void ParkingTest_Large_ParkSuccess()
    {
        _dataContext.Database.EnsureDeleted();
        _dataContext.Database.EnsureCreated();
        _dataContext.Add(DataHelper.GetValidGarage(2, 2, 20));
        _dataContext.SaveChanges();
        var garage = _dataContext.Garages.First();
        var toPark = new Vehicle() {LicensePlate = "TestPlate", Type = VehicleType.Large};
        var spaces = garage.FindParkingSpace(toPark);
        if(spaces.Count==0) Assert.Fail("Space not found.");
        spaces.ForEach(s=>s.Vehicle = toPark);
        _dataContext.SaveChanges();
        toPark = _dataContext.Vehicles.FirstOrDefault();
        if(toPark==default) Assert.Fail("Vehicle Not Saved");
        if(toPark.ParkingSpaces.Count==0) Assert.Fail("Vehicle Not Parked");
        if(toPark.ParkingSpaces.First().Type!=ParkingSpaceType.Large)
            Assert.Fail("Parked in wrong spot.");
        Assert.Pass("Successfully Parked");
    }
    [Test]
    public void ParkingTest_Large_FailNoSpaces()
    {
        _dataContext.Database.EnsureDeleted();
        _dataContext.Database.EnsureCreated();
        var garage = DataHelper.GetValidGarage(2, 2, 20);
        garage.ParkingSpaces.ToList().ForEach(p=>p.Type=ParkingSpaceType.Compact);
        _dataContext.Add(garage);
        _dataContext.SaveChanges();
        garage = _dataContext.Garages.First();
        var toPark = new Vehicle() {LicensePlate = "TestPlate", Type = VehicleType.Large};
        var spaces = garage.FindParkingSpace(toPark);
        if(spaces.Count==0) Assert.Pass("Space not found.");
        else Assert.Fail("Car parked with no space available.");
    }
    [Test]
    public void ParkingTest_Bus_FailNoSpaces()
    {
        _dataContext.Database.EnsureDeleted();
        _dataContext.Database.EnsureCreated();
        var garage = DataHelper.GetValidGarage(2, 2, 10);
        garage.ParkingSpaces.ToList().ForEach(p=>p.Type=ParkingSpaceType.Compact);
        _dataContext.Add(garage);
        _dataContext.SaveChanges();
        garage = _dataContext.Garages.First();
        var toPark = new Vehicle() {LicensePlate = "TestPlate", Type = VehicleType.Bus};
        var spaces = garage.FindParkingSpace(toPark);
        if(spaces.Count==0) Assert.Pass("Space not found.");
        else Assert.Fail("Car parked with no space available.");
    }
    [Test]
    public void ParkingTest_Bus_FailFullSpaces()
    {
        _dataContext.Database.EnsureDeleted();
        _dataContext.Database.EnsureCreated();
        _dataContext.Add(DataHelper.GetValidGarage(1, 1, 18));
        _dataContext.SaveChanges();
        var garage = _dataContext.Garages.First();
        var toPark = new Vehicle() {LicensePlate = "TestPlate", Type = VehicleType.Bus};
        var spaces = garage.FindParkingSpace(toPark);
        if(spaces.Count==0 || (int)spaces.Count<(int)VehicleType.Bus) Assert.Fail("Space not found.");
        spaces.ForEach(s=>s.Vehicle = toPark);
        _dataContext.SaveChanges();
        var toParkNext = new Vehicle() {LicensePlate = "TestPlate2", Type = VehicleType.Bus};
        spaces = garage.FindParkingSpace(toParkNext);
        if(spaces.Count==0) Assert.Pass("Lot Full");
        else Assert.Fail("Successfully Parked in full lot");
    }
    [Test]
    public void ParkingTest_Bus_ParkSuccess()
    {
        _dataContext.Database.EnsureDeleted();
        _dataContext.Database.EnsureCreated();
        _dataContext.Add(DataHelper.GetValidGarage(2, 2, 20));
        _dataContext.SaveChanges();
        var garage = _dataContext.Garages.First();
        var toPark = new Vehicle() {LicensePlate = "TestPlate", Type = VehicleType.Bus};
        var spaces = garage.FindParkingSpace(toPark);
        if(spaces.Count==0 || (int)spaces.Count<(int)VehicleType.Bus) Assert.Fail("Space not found.");
        spaces.ForEach(s=>s.Vehicle = toPark);
        _dataContext.SaveChanges();
        toPark = _dataContext.Vehicles.FirstOrDefault();
        if(toPark==default) Assert.Fail("Vehicle Not Saved");
        if(toPark.ParkingSpaces.Count==0) Assert.Fail("Vehicle Not Parked");
        if(toPark.ParkingSpaces.Any(p=>p.Type!=ParkingSpaceType.Large))
            Assert.Fail("Parked in wrong spot.");
        Assert.Pass("Successfully Parked");
    }
    [Test]
    public void ParkingTest_Validation_FailNoLicensePlate()
    {
        try
        {
            _dataContext.Database.EnsureDeleted();
            _dataContext.Database.EnsureCreated();
            _dataContext.Add(DataHelper.GetValidGarage(2, 2, 20));
            _dataContext.SaveChanges();
            var garage = _dataContext.Garages.First();
            var toPark = new Vehicle() {LicensePlate = "", Type = VehicleType.Large};
            var spaces = garage.FindParkingSpace(toPark);
            if (spaces.Count == 0) Assert.Fail("Space not found.");
            spaces.ForEach(s => s.Vehicle = toPark);
            _dataContext.Vehicles.Add(toPark);
            _dataContext.SaveChanges();
            Assert.Fail("Successfully Parked");
        }
        catch (DataValidationException ex)
        {
            Assert.Pass(ex.Message);
        }
    }
}