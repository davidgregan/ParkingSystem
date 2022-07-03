using System.Diagnostics.CodeAnalysis;
using EFCache;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using ParkingSystem.Api.Models.Entities;

namespace ParkingSystem.Api.Models;
/// <summary>
/// Entity framework data context use to interface with the database. the following data validation rules apply to
/// saved entities.
/// Garage:
/// 1) Must contain parking spaces.
/// 2) Must not contain currently parked vehicles to be saved or updated.
/// 3) All adjacent parking spaces must have adjacent space numbers and must not span multiple floors or rows.
///
/// Vehicle:
/// 1) Must have a non-null license plate.
/// 2) Must have a valid vehicle type.
///
/// Parking Space
/// 1) Must have a valid vehicle type.
/// </summary>
public class ParkingSystemDataContext : DbContext
{
    public ParkingSystemDataContext(DbContextOptions<ParkingSystemDataContext> options) : base(options)
    {

    }

    public virtual DbSet<Garage> Garages => Set<Garage>();
    public virtual DbSet<ParkingSpace> ParkingSpaces => Set<ParkingSpace>();
    public virtual DbSet<Vehicle> Vehicles => Set<Vehicle>();
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        EntityFrameworkCache.Initialize(new InMemoryCache());
        optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.DetachedLazyLoadingWarning));
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Garage>(entity =>
        {
            entity.HasKey(e => e.GarageId);
            entity.HasIndex(e => e.Name,"IX_Garage_Name")
                .IsUnique();
            entity.Property(e => e.GarageId).HasDefaultValue(Guid.NewGuid());
        });
        modelBuilder.Entity<ParkingSpace>(entity =>
        {
            entity.HasKey(e => e.ParkingSpaceId);
            entity.Property(e => e.ParkingSpaceId).HasDefaultValue(Guid.NewGuid());
            
            entity.HasIndex(e => e.GarageId, "IX_ParkingSpaces_GarageId");

            entity.HasIndex(e => e.LicensePlate, "IX_ParkingSpaces_LicensePlate");
            entity.HasIndex(e => e.Type, "IX_ParkingSpaces_Type");

            entity.HasIndex(e => e.NextSpaceId, "IX_ParkingSpaces_NextSpaceId")
                .IsUnique();

            entity.HasOne(d => d.Garage)
                .WithMany(p => p.ParkingSpaces)
                .HasForeignKey(d => d.GarageId);

            entity.HasOne(d => d.Vehicle)
                .WithMany(p => p.ParkingSpaces)
                .HasForeignKey(d => d.LicensePlate)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.NextSpace)
                .WithOne(p => p.PreviousSpace)
                .HasForeignKey<ParkingSpace>(d => d.NextSpaceId);
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.LicensePlate);
            entity.HasMany(e => e.ParkingSpaces)
                .WithOne(e => e.Vehicle)
                .HasForeignKey(p => p.LicensePlate)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
    public override int SaveChanges()
    {
        Validate();
        return base.SaveChanges();
    }
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
    {
        Validate();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
    
    
    private void Validate()
    {
        ValidateGarages();
        ValidateVehicles();
    }
    private void ValidateVehicles()
    {
        var toEvaluate = ChangeTracker.Entries()
            .Where(c => c.Entity is Vehicle &&
                        (c.State == EntityState.Added || c.State == EntityState.Modified));
        if(toEvaluate.Any(change=> String.IsNullOrEmpty(((Vehicle)change.Entity).LicensePlate)))
            throw new DataValidationException("Cannot save vehicle with no license plate");
    }

    private void ValidateGarages()
    {
        var toEvaluate = ChangeTracker.Entries()
            .Where(c => c.Entity is Garage && 
                        (c.State==EntityState.Modified ||
                         c.State==EntityState.Added||
                         c.State==EntityState.Deleted))
            .ToList();
        foreach (var change in toEvaluate)
        {
            var garage = (Garage)change.Entity;
            if (!garage.ParkingSpaces.Any())
                throw new DataValidationException("Cannot save a garage with no spaces.");
            var isEmpty =
                !ParkingSpaces.Any(p => p.GarageId == garage.GarageId && !String.IsNullOrEmpty(p.LicensePlate))&&
                !(garage.ParkingSpaces!=default && garage.ParkingSpaces.Any(p=>p.Vehicle!=default));
            if (!isEmpty)
                throw new DataValidationException("Parking garage must be empty in order to save/delete.");
            garage.Validate();
        }
    }
}