using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using ParkingSystem.Api.Models;
using ParkingSystem.Api.Models.Entities;

namespace ParkingSystem.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class GarageController : ControllerBase
{
    private readonly ParkingSystemDataContext _dbContext;

    public GarageController(ParkingSystemDataContext dbContext)
    {
        this._dbContext = dbContext;
        dbContext.Database.EnsureCreated();
    }
    /// <summary>
    /// Retrieve a list of high level garage data. Related entities are not included
    /// </summary>
    [HttpGet]
    [Route("GetAll")]
    [ProducesResponseType(typeof(List<Garage>),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(String),StatusCodes.Status404NotFound)]
    public IActionResult GetAll()
    {
        var toReturn = _dbContext.Garages.AsNoTracking().ToList();
        if (toReturn.Count == 0)
            return NotFound();
        return Ok(toReturn);
    }
    /// <summary>
    /// Retrieves detailed data related to a single garage.
    /// </summary>
    /// <param name="garageId">Primary key of the garage to be retrieved.</param>
    [HttpGet]
    [Route("GetById")]
    [ProducesResponseType(typeof(Garage),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(String),StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid garageId)
    {
        var toReturn = _dbContext.Garages
            .Include(g=>g.ParkingSpaces)
            .ThenInclude(p=>p.Vehicle)
            .AsNoTracking()
            .FirstOrDefault(g => g.GarageId == garageId);
        if (toReturn == default) return NotFound();
        return Ok(toReturn);
    }
    
    /// <summary>
    /// Save a garage to the database. Existing garages will be overwritten.
    /// </summary>
    /// <param name="toSave"> A garage entry to be saved. The following data validation must be
    /// satisfied:
    /// 1) Garage must have no currently parked vehicles.
    /// 2) Garage must include parking spaces.
    /// 3) Parking space rows must have continues parking space numbers and must not span multiple floors.</param>
    /// <returns>Identifier of the saved garage</returns>
    [HttpPost]
    [Route("Save")]
    [ProducesResponseType(typeof(Guid),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(String),StatusCodes.Status400BadRequest)]
    public IActionResult Save(Garage toSave)
    {
        if (_dbContext.ParkingSpaces.Any(p => p.GarageId==toSave.GarageId && !String.IsNullOrEmpty(p.LicensePlate)))
            return BadRequest("Parking garage must be empty in order to save.");
        if (_dbContext.Garages.Any(g => g.Name == toSave.Name && g.GarageId != toSave.GarageId))
            return BadRequest($"Separate parking garage already exist with the name \"{toSave.Name}\"");
        try
        {
            var existingGarage = _dbContext.Garages.FirstOrDefault(g => g.GarageId == toSave.GarageId);
            if (existingGarage != default)
            {
                _dbContext.Garages.Remove(existingGarage);
                _dbContext.SaveChanges();
            }
            _dbContext.Garages.Add(toSave);
            _dbContext.SaveChanges();
            return Ok(toSave.GarageId);
        }
        catch (DataValidationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    /// <summary>
    /// Delete a garage from the database. Associated entities will be deleted.
    /// </summary>
    /// <param name="garageId">Identifier of garage to be deleted.</param>
    /// <returns>True on successful deletion</returns>
    [HttpDelete]
    [Route("Delete")]
    [ProducesResponseType(typeof(bool),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(String),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(String),StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid garageId)
    {
        try
        {
            var toDelete = _dbContext.Garages.Find(garageId);
            if (toDelete == default)
                return NotFound();
            _dbContext.Garages.Remove(toDelete);
            _dbContext.SaveChanges();
        }
        catch (DataValidationException ex)
        {
            return BadRequest(ex.Message);
        }
        return Ok(true);
    }

    /// <summary>
    /// Finds a space for a vehicle in the selected garage and parks the vehicle if space is available.
    /// </summary>
    /// <param name="toAdd">Vehicle to add. Must include type as follows:
    /// 0=Motorcycle
    /// 1=Small Vehicle
    /// 2=Large Vehicle
    /// 5=Bus
    /// </param>
    /// <param name="garageId">Identifier of garage to park vehicle in.</param>
    /// <returns>A list of parking spaces used to park the vehicle.</returns>
    [HttpPost]
    [Route("ParkVehicle")]
    [ProducesResponseType(typeof(List<ParkingSpace>),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(String),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(String),StatusCodes.Status404NotFound)]
    public IActionResult ParkVehicle(Vehicle toAdd, Guid garageId)
    {
        if (_dbContext.Vehicles.Any(v => v.LicensePlate == toAdd.LicensePlate))
            return BadRequest("Vehicle is already parked in the system.");
        var targetGarage = _dbContext.Garages.FirstOrDefault(g => g.GarageId == garageId);
        if (targetGarage == default)
            return NotFound("Requested garage not found");
        var targetParkingSpaces = targetGarage.FindParkingSpace(toAdd);
        if (targetParkingSpaces.Count==0)
            return NotFound("Unable to locate space for vehicle");
        targetParkingSpaces.ForEach(p=>p.Vehicle = toAdd);
        _dbContext.Vehicles.Add(toAdd);
        _dbContext.SaveChanges();
        return Ok(targetParkingSpaces);
    }
    
    /// <summary>
    /// Finds a space for a list of vehicles in the selected garage and parks the vehicle if space is available.
    /// </summary>
    /// <param name="toAdd">A list of vehicles to add. Must include type as follows:
    /// 0=Motorcycle
    /// 1=Small Vehicle
    /// 2=Large Vehicle
    /// 5=Bus
    /// </param>
    /// <param name="garageId">Identifier of garage to park vehicle in.</param>
    /// <returns>A list of vehicles with their ParkingSpaces property filled. Any vehicle returned without an empty
    /// ParkingSpaces property was not able to be parked.
    /// </returns>
    [HttpPost]
    [Route("ParkVehicles")]
    [ProducesResponseType(typeof(List<Vehicle>),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(String),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(String),StatusCodes.Status404NotFound)]
    public IActionResult ParkVehicles(List<Vehicle> toAdd, Guid garageId)
    {
        if (!toAdd.Any())
            return BadRequest("Vehicle list empty.");
        var plates = toAdd.Select(v => v.LicensePlate).ToList();
        if (_dbContext.Vehicles.Any(v => EF.Functions.Contains(plates,v.LicensePlate)))
            return BadRequest("List contains vehicle(s) already parked in the system.");
        var targetGarage = _dbContext.Garages.FirstOrDefault(g => g.GarageId == garageId);
        if (targetGarage == default)
            return NotFound("Requested garage not found");
        foreach (var vehicle in toAdd)
        {
            var targetParkingSpaces = targetGarage.FindParkingSpace(vehicle);
            if (targetParkingSpaces.Count != 0)
            {
                vehicle.ParkingSpaces = targetParkingSpaces;
                _dbContext.Vehicles.Add(vehicle);
            }
        }
        _dbContext.SaveChanges();
        return Ok(toAdd);
    }

    /// <summary>
    /// Removes a vehicle that is parked in the system and frees the spaces.
    /// </summary>
    /// <param name="toRemove">Vehicle entry to be removed from the system.</param>
    /// <returns>Returns true for successful removal.</returns>
    [HttpDelete]
    [Route("RemoveVehicle")]
    [ProducesResponseType(typeof(bool),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(String),StatusCodes.Status404NotFound)]
    public IActionResult RemoveVehicle(Vehicle toRemove)
    {
        var toDelete = _dbContext.Vehicles.FirstOrDefault(v=>v.LicensePlate == toRemove.LicensePlate);
        if (toDelete == default)
            return NotFound();
        _dbContext.Remove(toDelete);
        _dbContext.SaveChanges();
        return Ok(true);
    }
    /// <summary>
    /// Finds a vehicle located in the system.
    /// </summary>
    /// <param name="licensePlate">Vehicle entry to be searched.</param>
    /// <returns>Returns a vehicle entity with associated parking spaces and garage details.</returns>
    [HttpGet]
    [Route("FindVehicle")]
    [ProducesResponseType(typeof(Vehicle),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(String),StatusCodes.Status404NotFound)]
    public IActionResult FindVehicle(string licensePlate)
    {
        var toReturn = _dbContext.Vehicles
            .Include(v=>v.ParkingSpaces)
            .ThenInclude(p=>p.Garage)
            .AsNoTracking()
            .FirstOrDefault(v => v.LicensePlate == licensePlate);
        return Ok(toReturn);
    }
}