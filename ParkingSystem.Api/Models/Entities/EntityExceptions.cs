namespace ParkingSystem.Api.Models.Entities;

/// <summary>
/// Generic data validation exception.
/// </summary>
public class DataValidationException : Exception
{
    public DataValidationException()
    {
    }
    public DataValidationException(string message)
        : base(message)
    {
    }
}
/// <summary>
/// Exception occurs when a referential loop is detected in ParkingSpace relations.
/// </summary>
public class ReferenceLoopException : DataValidationException
{
    public ReferenceLoopException()
    {
    }
    public ReferenceLoopException(string message)
        : base(message)
    {
    }
}
/// <summary>
/// Exception occurs when required data is missing.
/// </summary>
public class MissingDataException : DataValidationException
{
    public MissingDataException()
    {
    }
    public MissingDataException(string message)
        : base(message)
    {
    }
}
/// <summary>
/// Exception occurs when a ParkingSpace fails it relationship with its neighbor. Failures include:
/// 1) ParkingSpace is located on a different floor than its neighbor.
/// 2) ParkingSpace is located in a different row than its neighbor.
/// 3) ParkingSpace number is not in sequence with its neighbor.
/// </summary>
public class ParkingSpaceAdjacencyException : DataValidationException
{
    public ParkingSpaceAdjacencyException()
    {
    }
    public ParkingSpaceAdjacencyException(string message)
        : base(message)
    {
    }
}