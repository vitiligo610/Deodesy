using Deodesy.Library.Helpers;

namespace Deodesy.Library;

/// <summary>
/// Represents a point on the Earth's surface using Latitude and Longitude.
/// </summary>
public class Coordinate
{
    private const double Tolerance = 1e-6; // accuracy of 10-15cm
    private double _latitude;
    private double _longitude;
    
    /// <summary>
    /// Gets or sets the latitude in degrees.
    /// The value is wrapped to the range -90 to 90.
    /// </summary>
    /// <example>
    /// <code>
    /// var coord = new Coordinate(95, 0);
    /// Console.WriteLine(coord.Latitude); // 85° N
    /// </code>
    /// </example>
    public double Latitude
    {
        get => _latitude;
        set => _latitude = Dms.Wrap90(value);
    }

    /// <summary>
    /// Gets or sets the longitude in degrees.
    /// The value is wrapped to the range -180 to 180.
    /// </summary>
    /// <example>
    /// <code>
    /// var coord = new Coordinate(0, 185);
    /// Console.WriteLine(coord.Longitude); // 182.5° E
    /// </code>
    /// </example>
    public double Longitude
    {
        get => _longitude;
        set => _longitude = Dms.Wrap180(value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Coordinate"/> class.
    /// </summary>
    /// <param name="latitude">The latitude in degrees (-90 to 90). Defaults to 0.</param>
    /// <param name="longitude">The longitude in degrees (-180 to 180). Defaults to 0.</param>
    /// <example>
    /// <code>
    /// var coord1 = new Coordinate(); // 0° N, 0° E
    /// var coord2 = new Coordinate(30, -120); // 30° N, 120° W
    /// </code>
    /// </example>
    public Coordinate(double latitude = 0, double longitude = 0)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>
    /// Returns Latitude in Radians.
    /// </summary>
    /// <example>
    /// <code>
    /// var coord = new Coordinate(90, 0);
    /// Console.WriteLine(coord.LatitudeR); // 1.5707963267948966 (PI/2)
    /// </code>
    /// </example>
    public double LatitudeR => Latitude.ToRadians();
    
    /// <summary>
    /// Returns Longitude in Radians.
    /// </summary>
    /// <example>
    /// <code>
    /// var coord = new Coordinate(0, 180);
    /// Console.WriteLine(coord.LongitudeR); // 3.141592653589793 (PI)
    /// </code>
    /// </example>
    public double LongitudeR => Longitude.ToRadians();

    /// <summary>
    /// Compares two <see cref="Coordinate"/> objects for equality.
    /// </summary>
    /// <param name="left">The first <see cref="Coordinate"/> to compare.</param>
    /// <param name="right">The second <see cref="Coordinate"/> to compare.</param>
    /// <returns><c>true</c> if the coordinates are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(Coordinate? left, Coordinate? right) =>
        ReferenceEquals(left, right) || (!ReferenceEquals(left, null) && left.Equals(right));
    
    /// <summary>
    /// Compares two <see cref="Coordinate"/> objects for inequality.
    /// </summary>
    /// <param name="left">The first <see cref="Coordinate"/> to compare.</param>
    /// <param name="right">The second <see cref="Coordinate"/> to compare.</param>
    /// <returns><c>true</c> if the coordinates are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(Coordinate left, Coordinate right) =>
        !(left == right);
    
    /// <summary>
    /// Determines whether the specified <see cref="Coordinate"/> object is equal to the current <see cref="Coordinate"/> object.
    /// </summary>
    /// <param name="other">The <see cref="Coordinate"/> object to compare with the current object.</param>
    /// <returns><c>true</c> if the specified <see cref="Coordinate"/> object is equal to the current object; otherwise, <c>false</c>.</returns>
    private bool Equals(Coordinate? other)
    {
        if (other is null) return false;
        return Math.Abs(other.Latitude - Latitude) < Tolerance && Math.Abs(other.Longitude - Longitude) < Tolerance;
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="Coordinate"/> object.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current object.</param>
    /// <returns><c>true</c> if the specified <see cref="object"/> is equal to the current object; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Coordinate)obj);
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Latitude, Longitude);
    }
}
