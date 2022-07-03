using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ParkingSystem.Api.Models;
using ParkingGarage.Api.Tests.Helpers;
using ParkingSystem.Api.Controllers;
using ParkingSystem.Api.Models.Entities;
namespace ParkingGarage.Api.Tests;
#nullable disable
public class ControllerTests
{
    private GarageController _controller = null!;
    private ParkingSystemDataContext _dataContext = null!;
    
    [SetUp]
    public void Setup()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ParkingSystemDataContext>();
        optionsBuilder.UseLazyLoadingProxies();
        optionsBuilder.UseInMemoryDatabase("ParkingSystemTestDB");
        _dataContext = new ParkingSystemDataContext(optionsBuilder.Options);
        _controller = new GarageController(_dataContext);
    }

    [Test]
    public void ApiCall_GetGarages_Success()
    {
        _dataContext.Database.EnsureDeleted();
        _dataContext.Database.EnsureCreated();
        var testGarage = DataHelper.GetValidGarage(4, 4, 10);
        _dataContext.Garages.Add(testGarage);
        _dataContext.SaveChanges();
        var result = _controller.GetAll();
        if(result is not OkObjectResult) Assert.Fail("Unexpected response from controller.");
        var returnedGarage = (List<Garage>) ((OkObjectResult) result).Value;
        if(returnedGarage.Any(g=>g.GarageId == testGarage.GarageId))
            Assert.Pass("API Call succeeded.");
    }
    [Test]
    public void ApiCall_GetGarageById_Success()
    {
        _dataContext.Database.EnsureDeleted();
        _dataContext.Database.EnsureCreated();
        var testGarage = DataHelper.GetValidGarage(4, 4, 10);
        _dataContext.Garages.Add(testGarage);
        _dataContext.SaveChanges();
        var result = _controller.GetById(testGarage.GarageId);
        if(result is not OkObjectResult) Assert.Fail("Unexpected response from controller.");
        var returnedGarage = (Garage)((OkObjectResult) result).Value;
        if(returnedGarage.GarageId == testGarage.GarageId)
            Assert.Pass("API Call succeeded.");
    }
    [Test]
    public void ApiCall_SaveGarage_Success()
    {
        var testGarage = new Garage() { Name = "TestGarage"};
        testGarage.ParkingSpaces = new List<ParkingSpace>();
        var space1 = new ParkingSpace() {FloorNumber = 1, RowNumber = 0, SpaceNumber = 1 };
        var space2 = new ParkingSpace() {FloorNumber = 1, RowNumber = 0, SpaceNumber = 2 };
        var space3 = new ParkingSpace() {FloorNumber = 1, RowNumber = 0, SpaceNumber = 3 };
        space1.NextSpace = space2;
        space2.NextSpace = space3;
        testGarage.ParkingSpaces= new List<ParkingSpace>(){space1,space2,space3};
        _dataContext.Add(testGarage);
        _dataContext.SaveChanges();
        _dataContext.Entry(testGarage).State = EntityState.Detached;
        var newName = "Updated Garage";
        testGarage.Name = newName;
        testGarage.ParkingSpaces = testGarage.ParkingSpaces.Take(2).ToList();
        space2.NextSpace = default;
        testGarage.ParkingSpaces.Remove(space3);
        var result = _controller.Save(testGarage);
        if(result is not OkObjectResult) Assert.Fail("Unexpected response from controller.");
        var returnedId = (Guid)((OkObjectResult) result).Value;
        var savedGarage = _dataContext.Garages.First(g => g.GarageId == returnedId);
        if(savedGarage.Name == newName && !savedGarage.ParkingSpaces.Contains(space3)) 
            Assert.Pass("API Call succeeded.");
        else
            Assert.Fail("Saved data does not match sent data.");
    }
    [Test]
    public void ApiCall_DeleteGarage_Success()
    {
        _dataContext.Database.EnsureDeleted();
        _dataContext.Database.EnsureCreated();
        var testGarage = DataHelper.GetValidGarage(4, 4, 10);
        _dataContext.Garages.Add(testGarage);
        _dataContext.SaveChanges();
        var result = _controller.Delete(testGarage.GarageId);
        if(result is not OkObjectResult) Assert.Fail("Unexpected response from controller.");
        var returnedGarage = (bool)((OkObjectResult) result).Value;
        if(_dataContext.Garages.Any()||_dataContext.ParkingSpaces.Any())
            Assert.Fail("Test Garage was not deleted.");
        Assert.Pass("API Call Successed");
    }
    [Test]
    public void ApiCall_ParkVehicle_Success()
    {
        _dataContext.Database.EnsureDeleted();
        _dataContext.Database.EnsureCreated();
        var testGarage = DataHelper.GetValidGarage(4, 4, 10);
        _dataContext.Garages.Add(testGarage);
        _dataContext.SaveChanges();
        var testVehicle = new Vehicle() {LicensePlate = "TEST1", Type = VehicleType.Motorcycle};
        var result = _controller.ParkVehicle(testVehicle,testGarage.GarageId);
        if(result is not OkObjectResult) Assert.Fail("Unexpected response from controller.");
        var returnedSpaces = (List<ParkingSpace>)((OkObjectResult) result).Value;
        var savedSpace = _dataContext.ParkingSpaces.FirstOrDefault(p => p.Vehicle == testVehicle);
        if(_dataContext.ParkingSpaces.Any(p=>p.Vehicle==testVehicle))
            Assert.Pass("API Call succeeded.");
        else
            Assert.Fail("Saved data does not contain parked car.");
    }
    [Test]
    public void ApiCall_ParkVehicles_Success()
    {
        _dataContext.Database.EnsureDeleted();
        _dataContext.Database.EnsureCreated();
        var testGarage = DataHelper.GetValidGarage(5, 10, 100);
        testGarage.Name = "Test Garage";
        _dataContext.Garages.Add(testGarage);
        _dataContext.SaveChanges();
        var startTime = DateTime.Now;
        var toPark = new List<Vehicle>();
        for (int i = 1; i <= 5000; i++)
        {
            var newVehicle = new Vehicle() {LicensePlate = $"VEHICLE{i}", Type = VehicleType.Motorcycle};
            toPark.Add(newVehicle);
        }
        var result = _controller.ParkVehicles(toPark, testGarage.GarageId);
        if(result is not OkObjectResult) Assert.Fail("Unexpected response from controller.");
        var returned = (List<Vehicle>)((OkObjectResult) result).Value;
        if(returned.Any(v=>!v.ParkingSpaces.Any())) Assert.Fail("Not all cars parked");
        Assert.Pass("Parked all vehicles.");
    }
    [Test]
    public void ApiCall_RemoveVehicle_Success()
    {
        _dataContext.Database.EnsureDeleted();
        _dataContext.Database.EnsureCreated();
        var testGarage = DataHelper.GetValidGarage(4, 4, 10);
        _dataContext.Garages.Add(testGarage);
        _dataContext.SaveChanges();
        var testVehicle = new Vehicle() {LicensePlate = "TEST1", Type = VehicleType.Motorcycle};
        _controller.ParkVehicle(testVehicle,testGarage.GarageId);
        var result = _controller.RemoveVehicle(testVehicle);
        if(result is not OkObjectResult) Assert.Fail("Unexpected response from controller.");
        var returnedSpaces = (bool)((OkObjectResult) result).Value;
        if(!_dataContext.ParkingSpaces.Any(p=>p.Vehicle==testVehicle))
            Assert.Pass("API Call succeeded.");
        else
            Assert.Fail("Parked car was not removed.");
    }
}