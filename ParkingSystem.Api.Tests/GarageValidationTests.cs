using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ParkingSystem.Api.Models;
using ParkingGarage.Api.Tests.Helpers;
using ParkingSystem.Api.Models.Entities;
namespace ParkingGarage.Api.Tests;

public class GarageValidationTests
{
    private ParkingSystemDataContext _dataContext = null!;
    [SetUp]
    public void Setup()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ParkingSystemDataContext>();
        optionsBuilder.UseLazyLoadingProxies();
        optionsBuilder.UseInMemoryDatabase("ParkingSystemTestDB");
        _dataContext = new ParkingSystemDataContext(optionsBuilder.Options);
    }

    [Test]
    public void SaveGarage_FailValidation_NoParkingSpaces()
    {
        _dataContext.Database.EnsureDeleted();
        _dataContext.Database.EnsureCreated();
        try
        {
            var toAdd = new Garage() { Name = "TestGarage"};
            _dataContext.Add(toAdd);
            _dataContext.SaveChanges();
        }
        catch (ParkingSystem.Api.Models.Entities.DataValidationException ex)
        {
            Assert.Pass(ex.Message);
        }
        Assert.Fail("Successfully saved an invalid state.");
    }
    [Test]
    public void SaveGarage_FailValidation_LoopedReference()
    {
        _dataContext.Database.EnsureDeleted();
        _dataContext.Database.EnsureCreated();
        try
        {
            var toAdd = new Garage() { Name = "TestGarage"};
            toAdd.ParkingSpaces = new List<ParkingSpace>();
            var space1 = new ParkingSpace() {FloorNumber = 1, RowNumber = 0, SpaceNumber = 1 };
            var space2 = new ParkingSpace() {FloorNumber = 1, RowNumber = 0, SpaceNumber = 2 };
            var space3 = new ParkingSpace() {FloorNumber = 1, RowNumber = 0, SpaceNumber = 3 };
            space1.NextSpace = space2;
            space2.NextSpace = space3;
            space3.NextSpace = space1;
            toAdd.ParkingSpaces.Add(space1);
            toAdd.ParkingSpaces.Add(space2);
            toAdd.ParkingSpaces.Add(space3);
            _dataContext.Add(toAdd);
            _dataContext.SaveChanges();
        }
        catch (ParkingSystem.Api.Models.Entities.ReferenceLoopException ex)
        {
            Assert.Pass(ex.Message);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
        Assert.Fail("Successfully saved an invalid state.");
    }
    [Test]
    public void SaveGarage_FailValidation_SpaceAdjacency()
    {
        _dataContext.Database.EnsureDeleted();
        _dataContext.Database.EnsureCreated();
        try
        {
            var toAdd = new Garage() { Name = "TestGarage"};
            toAdd.ParkingSpaces = new List<ParkingSpace>();
            var space1 = new ParkingSpace() {FloorNumber = 1, RowNumber = 0, SpaceNumber = 1};
            var space2 = new ParkingSpace() {FloorNumber = 2, RowNumber = 0, SpaceNumber = 3};
            var space3 = new ParkingSpace() {FloorNumber = 1, RowNumber = 0, SpaceNumber = 2};
            space1.NextSpace = space2;
            space2.NextSpace = space3;
            toAdd.ParkingSpaces.Add(space1);
            toAdd.ParkingSpaces.Add(space2);
            toAdd.ParkingSpaces.Add(space3);
            _dataContext.Add(toAdd);
            _dataContext.SaveChanges();
        }
        catch (ParkingSystem.Api.Models.Entities.ParkingSpaceAdjacencyException ex)
        {
            Assert.Pass(ex.Message);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
        Assert.Fail("Successfully saved an invalid state.");
    }
    [Test]
    public void SaveGarage_FailValidation_FloorAdjacency()
    {
        _dataContext.Database.EnsureDeleted();
        _dataContext.Database.EnsureCreated();
        try
        {
            var toAdd = new Garage() { Name = "TestGarage"};
            toAdd.ParkingSpaces = new List<ParkingSpace>();
            var space1 = new ParkingSpace() {FloorNumber = 1, RowNumber = 0, SpaceNumber = 1};
            var space2 = new ParkingSpace() {FloorNumber = 2, RowNumber = 0, SpaceNumber = 2};
            var space3 = new ParkingSpace() {FloorNumber = 1, RowNumber = 0, SpaceNumber = 3};
            space1.NextSpace = space2;
            space2.NextSpace = space3;
            toAdd.ParkingSpaces.Add(space1);
            toAdd.ParkingSpaces.Add(space2);
            toAdd.ParkingSpaces.Add(space3);
            _dataContext.Add(toAdd);
            _dataContext.SaveChanges();
        }
        catch (ParkingSystem.Api.Models.Entities.ParkingSpaceAdjacencyException ex)
        {
            Assert.Pass(ex.Message);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
        Assert.Fail("Successfully saved an invalid state.");
    }
    [Test]
    public void SaveGarage_FailValidation_RowAdjacency()
    {
        _dataContext.Database.EnsureDeleted();
        _dataContext.Database.EnsureCreated();
        try
        {
            var toAdd = new Garage() { Name = "TestGarage"};
            toAdd.ParkingSpaces = new List<ParkingSpace>();
            var space1 = new ParkingSpace() {FloorNumber = 1, RowNumber = 0, SpaceNumber = 1};
            var space2 = new ParkingSpace() {FloorNumber = 1, RowNumber = 1, SpaceNumber = 2};
            var space3 = new ParkingSpace() {FloorNumber = 1, RowNumber = 0, SpaceNumber = 3};
            space1.NextSpace = space2;
            space2.NextSpace = space3;
            toAdd.ParkingSpaces.Add(space1);
            toAdd.ParkingSpaces.Add(space2);
            toAdd.ParkingSpaces.Add(space3);
            _dataContext.Add(toAdd);
            _dataContext.SaveChanges();
        }
        catch (ParkingSystem.Api.Models.Entities.ParkingSpaceAdjacencyException ex)
        {
            Assert.Pass(ex.Message);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
        Assert.Fail("Successfully saved an invalid state.");
    }
    [Test]
    public void SaveGarage_FailValidation_FailGarageNotEmpty()
    {
        _dataContext.Database.EnsureDeleted();
        _dataContext.Database.EnsureCreated();
        try
        {
            var toAdd = new Garage() { Name = "TestGarage"};
            toAdd.ParkingSpaces = new List<ParkingSpace>();
            var space1 = new ParkingSpace() {FloorNumber = 1, RowNumber = 0, SpaceNumber = 1};
            var space2 = new ParkingSpace() {FloorNumber = 1, RowNumber = 0, SpaceNumber = 2};
            var space3 = new ParkingSpace() {FloorNumber = 1, RowNumber = 0, SpaceNumber = 3};
            space1.NextSpace = space2;
            space2.NextSpace = space3;
            toAdd.ParkingSpaces.Add(space1);
            toAdd.ParkingSpaces.Add(space2);
            toAdd.ParkingSpaces.Add(space3);
            space1.Vehicle = new Vehicle() { LicensePlate = "TestPlate", Type = 0};
            _dataContext.Add(toAdd);
            _dataContext.SaveChanges();
        }
        catch (ParkingSystem.Api.Models.Entities.DataValidationException ex)
        {
            Assert.Pass(ex.Message);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
        Assert.Fail("Successfully saved an invalid state.");
    }
    [Test]
    public void SaveGarage_Validation_Pass()
    {
        _dataContext.Database.EnsureDeleted();
        _dataContext.Database.EnsureCreated();
        try
        {
            var toSave = DataHelper.GetValidGarage(2, 2, 10);
            _dataContext.Add(toSave);
            _dataContext.SaveChanges();
            var retrievedGarage = _dataContext.Garages.FirstOrDefault(g => g.GarageId == toSave.GarageId);
            if(retrievedGarage== default) Assert.Fail();
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
        Assert.Pass();
    }
    
}